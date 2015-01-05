Module MATMST_IOH

    Sub Main()

        Const processCode As String = "MATMST_IOH"

        Const processInstanceToProcess As Integer = 1
        Const processInstanceAfterProcess As Integer = 2

        ' default values
        Dim nItemPerBatch As Integer = 10000
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
            s1 = My.Settings(propertyName:="MATMST_IOH_GetBatchSize")
        Catch ex As Exception
            s1 = "0"
        End Try
        If CInt(s1) > 0 Then
            nItemPerBatch = CInt(s1)
        End If
        Try
            s1 = My.Settings(propertyName:="MATMST_IOH_MaxReq")
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

        Dim sb As System.Text.StringBuilder = Nothing
        Dim sql As String = ""
        Dim oBatch As New MATMST_IOH_batch
        Dim sr As System.IO.StreamReader = Nothing
        Dim s As String = ""

        Dim cmdRead As OleDb.OleDbCommand = Nothing
        Dim oraCN1 As New OleDb.OleDbConnection(oraCNString)

        oraCN1.Open()

        If (oraCN1.State = ConnectionState.Open) Then

            logger.WriteVerboseLog("using DB : " & oraCN1.DataSource)

            '
            ' grab NEW items from PS_ISA_MB_MATMAST table
            '   we'll use list and try to grab IOH information
            '
            sr = New System.IO.StreamReader(CStr(My.Settings("MATMST_IOH_SelectNewItemToProc")))
            s = sr.ReadToEnd
            sr.Dispose()
            sr = Nothing

            ' fields 
            '   customer Id         {0}
            '   process instance    {1}
            sb = New System.Text.StringBuilder
            sb.AppendFormat(s, custId, processInstanceToProcess.ToString)
            s = sb.ToString
            sb = Nothing

            cmdRead = oraCN1.CreateCommand
            cmdRead.CommandText = s
            cmdRead.CommandType = CommandType.Text

            Dim dr As OleDb.OleDbDataReader = Nothing

            Try
                dr = cmdRead.ExecuteReader
            Catch ex As Exception
                logger.WriteErrorLog("errorred retrieving NEWLY added part/s from material master to retrieve IOH info for.")
                Try
                    dr.Close()
                Catch ex1 As Exception
                End Try
                dr = Nothing
            End Try

            If Not (dr Is Nothing) Then
                If dr.HasRows Then

                    Dim arrKeyValue As Hashtable = Nothing

                    Dim iBatch As String = ""

                    Dim sCUST_ID As String = ""
                    Dim sPLANT As String = ""
                    Dim sINV_ITEM_ID As String = ""
                    Dim sEFF_STATUS As String = ""
                    Dim sISA_CUSTOMER_CAT As String = ""
                    Dim sIM_CFFT As String = ""
                    Dim sINV_STOCK_TYPE As String = ""
                    Dim sISA_CUSTOMER_MFG As String = ""
                    Dim sMFG_ITM_ID As String = ""
                    Dim sPREFERRED_MFG As String = ""
                    Dim nREORDER_POINT As Decimal = 0
                    Dim nQTY_MAXIMUM As Decimal = 0
                    Dim nREORDER_QTY As Decimal = 0
                    Dim sUNIT_OF_MEASURE As String = ""
                    Dim sREPAIR_OPTION_LBL As String = ""
                    Dim nSTANDARD_COST As Decimal = 0
                    Dim nSTD_LEAD As Decimal = 0
                    Dim sUTILIZ_CD As String = ""
                    Dim sISA_NRTE_GL_CODE As String = ""
                    Dim sISA_ARTE_GL_CODE As String = ""
                    Dim sISA_TRTE_GL_CODE As String = ""
                    Dim sISA_NHOT_GL_CODE As String = ""
                    Dim sISA_MN_TAX_CODE As String = ""
                    Dim sISA_NC_TAX_CODE As String = ""
                    Dim sISA_UT_TAX_CODE As String = ""
                    Dim nPROCESS_INSTANCE As Integer = 0

                    Dim nRecId As Integer = 0
                    Dim sKey As String = ""

                    Dim sUpdateKeyColumns As String = CStr(My.Settings("MATMST_IOH_UpdateKeyColumns"))

                    While dr.Read
                        nRecId += 1

                        ' grab row values
                        sCUST_ID = ""
                        Try
                            'sCUST_ID = CStr(dr("CUST_ID")).Trim.ToUpper
                            sCUST_ID = CStr(dr("CUST_ID"))
                        Catch ex As Exception
                        End Try
                        If (sCUST_ID Is Nothing) Then
                            sCUST_ID = ""
                        End If

                        sPLANT = ""
                        Try
                            'sPLANT = CStr(dr("PLANT")).Trim
                            sPLANT = CStr(dr("PLANT"))
                        Catch ex As Exception
                        End Try
                        If (sPLANT Is Nothing) Then
                            sPLANT = ""
                        End If

                        sINV_ITEM_ID = ""
                        Try
                            'sINV_ITEM_ID = CStr(dr("INV_ITEM_ID")).Trim
                            sINV_ITEM_ID = CStr(dr("INV_ITEM_ID"))
                        Catch ex As Exception
                        End Try
                        If (sINV_ITEM_ID Is Nothing) Then
                            sINV_ITEM_ID = ""
                        End If

                        sEFF_STATUS = ""
                        Try
                            'sEFF_STATUS = CStr(dr("EFF_STATUS")).Trim
                            sEFF_STATUS = CStr(dr("EFF_STATUS"))
                        Catch ex As Exception
                        End Try
                        If (sEFF_STATUS Is Nothing) Then
                            sEFF_STATUS = ""
                        End If

                        sISA_CUSTOMER_CAT = ""
                        Try
                            'sISA_CUSTOMER_CAT = CStr(dr("ISA_CUSTOMER_CAT")).Trim
                            sISA_CUSTOMER_CAT = CStr(dr("ISA_CUSTOMER_CAT"))
                        Catch ex As Exception
                        End Try
                        If (sISA_CUSTOMER_CAT Is Nothing) Then
                            sISA_CUSTOMER_CAT = ""
                        End If

                        sIM_CFFT = ""
                        Try
                            'sIM_CFFT = CStr(dr("IM_CFFT")).Trim
                            sIM_CFFT = CStr(dr("IM_CFFT"))
                        Catch ex As Exception
                        End Try
                        If (sIM_CFFT Is Nothing) Then
                            sIM_CFFT = ""
                        End If

                        sINV_STOCK_TYPE = ""
                        Try
                            'sINV_STOCK_TYPE = CStr(dr("INV_STOCK_TYPE")).Trim
                            sINV_STOCK_TYPE = CStr(dr("INV_STOCK_TYPE"))
                        Catch ex As Exception
                        End Try
                        If (sINV_STOCK_TYPE Is Nothing) Then
                            sINV_STOCK_TYPE = ""
                        End If

                        sISA_CUSTOMER_MFG = ""
                        Try
                            'sISA_CUSTOMER_MFG = CStr(dr("ISA_CUSTOMER_MFG")).Trim
                            sISA_CUSTOMER_MFG = CStr(dr("ISA_CUSTOMER_MFG"))
                        Catch ex As Exception
                        End Try
                        If (sISA_CUSTOMER_MFG Is Nothing) Then
                            sISA_CUSTOMER_MFG = ""
                        End If

                        sMFG_ITM_ID = ""
                        Try
                            'sMFG_ITM_ID = CStr(dr("MFG_ITM_ID")).Trim
                            sMFG_ITM_ID = CStr(dr("MFG_ITM_ID"))
                        Catch ex As Exception
                        End Try
                        If (sMFG_ITM_ID Is Nothing) Then
                            sMFG_ITM_ID = ""
                        End If

                        sPREFERRED_MFG = ""
                        Try
                            'sPREFERRED_MFG = CStr(dr("sPREFERRED_MFG")).Trim
                            sPREFERRED_MFG = CStr(dr("sPREFERRED_MFG"))
                        Catch ex As Exception
                        End Try
                        If (sPREFERRED_MFG Is Nothing) Then
                            sPREFERRED_MFG = ""
                        End If

                        nREORDER_POINT = 0
                        Try
                            nREORDER_POINT = CDec(dr("REORDER_POINT"))
                        Catch ex As Exception
                        End Try

                        nQTY_MAXIMUM = 0
                        Try
                            nQTY_MAXIMUM = CDec(dr("QTY_MAXIMUM"))
                        Catch ex As Exception
                        End Try

                        nREORDER_QTY = 0
                        Try
                            nREORDER_QTY = CDec(dr("REORDER_QTY"))
                        Catch ex As Exception
                        End Try

                        sUNIT_OF_MEASURE = ""
                        Try
                            'sUNIT_OF_MEASURE = CStr(dr("UNIT_OF_MEASURE")).Trim
                            sUNIT_OF_MEASURE = CStr(dr("UNIT_OF_MEASURE"))
                        Catch ex As Exception
                        End Try
                        If (sUNIT_OF_MEASURE Is Nothing) Then
                            sUNIT_OF_MEASURE = ""
                        End If

                        sREPAIR_OPTION_LBL = ""
                        Try
                            'sREPAIR_OPTION_LBL = CStr(dr("REPAIR_OPTION_LBL")).Trim
                            sREPAIR_OPTION_LBL = CStr(dr("REPAIR_OPTION_LBL"))
                        Catch ex As Exception
                        End Try
                        If (sREPAIR_OPTION_LBL Is Nothing) Then
                            sREPAIR_OPTION_LBL = ""
                        End If

                        nSTANDARD_COST = 0
                        Try
                            nSTANDARD_COST = CDec(dr("nSTANDARD_COST"))
                        Catch ex As Exception
                        End Try

                        nSTD_LEAD = 0
                        Try
                            nSTD_LEAD = CDec(dr("STD_LEAD"))
                        Catch ex As Exception
                        End Try

                        sUTILIZ_CD = ""
                        Try
                            'sUTILIZ_CD = CStr(dr("UTILIZ_CD")).Trim
                            sUTILIZ_CD = CStr(dr("UTILIZ_CD"))
                        Catch ex As Exception
                        End Try
                        If (sUTILIZ_CD Is Nothing) Then
                            sUTILIZ_CD = ""
                        End If

                        sISA_NRTE_GL_CODE = ""
                        Try
                            'sISA_NRTE_GL_CODE = CStr(dr("ISA_NRTE_GL_CODE")).Trim
                            sISA_NRTE_GL_CODE = CStr(dr("ISA_NRTE_GL_CODE"))
                        Catch ex As Exception
                        End Try
                        If (sISA_NRTE_GL_CODE Is Nothing) Then
                            sISA_NRTE_GL_CODE = ""
                        End If

                        sISA_ARTE_GL_CODE = ""
                        Try
                            'sISA_ARTE_GL_CODE = CStr(dr("ISA_ARTE_GL_CODE")).Trim
                            sISA_ARTE_GL_CODE = CStr(dr("ISA_ARTE_GL_CODE"))
                        Catch ex As Exception
                        End Try
                        If (sISA_ARTE_GL_CODE Is Nothing) Then
                            sISA_ARTE_GL_CODE = ""
                        End If

                        sISA_TRTE_GL_CODE = ""
                        Try
                            'sISA_TRTE_GL_CODE = CStr(dr("ISA_TRTE_GL_CODE")).Trim
                            sISA_TRTE_GL_CODE = CStr(dr("ISA_TRTE_GL_CODE"))
                        Catch ex As Exception
                        End Try
                        If (sISA_TRTE_GL_CODE Is Nothing) Then
                            sISA_TRTE_GL_CODE = ""
                        End If

                        sISA_NHOT_GL_CODE = ""
                        Try
                            'sISA_NHOT_GL_CODE = CStr(dr("ISA_NHOT_GL_CODE")).Trim
                            sISA_NHOT_GL_CODE = CStr(dr("ISA_NHOT_GL_CODE"))
                        Catch ex As Exception
                        End Try
                        If (sISA_NHOT_GL_CODE Is Nothing) Then
                            sISA_NHOT_GL_CODE = ""
                        End If

                        sISA_MN_TAX_CODE = ""
                        Try
                            'sISA_MN_TAX_CODE = CStr(dr("ISA_MN_TAX_CODE")).Trim
                            sISA_MN_TAX_CODE = CStr(dr("ISA_MN_TAX_CODE"))
                        Catch ex As Exception
                        End Try
                        If (sISA_MN_TAX_CODE Is Nothing) Then
                            sISA_MN_TAX_CODE = ""
                        End If

                        sISA_NC_TAX_CODE = ""
                        Try
                            'sISA_NC_TAX_CODE = CStr(dr("ISA_NC_TAX_CODE")).Trim
                            sISA_NC_TAX_CODE = CStr(dr("ISA_NC_TAX_CODE"))
                        Catch ex As Exception
                        End Try
                        If (sISA_NC_TAX_CODE Is Nothing) Then
                            sISA_NC_TAX_CODE = ""
                        End If

                        sISA_UT_TAX_CODE = ""
                        Try
                            'sISA_UT_TAX_CODE = CStr(dr("ISA_UT_TAX_CODE")).Trim
                            sISA_UT_TAX_CODE = CStr(dr("ISA_UT_TAX_CODE"))
                        Catch ex As Exception
                        End Try
                        If (sISA_UT_TAX_CODE Is Nothing) Then
                            sISA_UT_TAX_CODE = ""
                        End If

                        nPROCESS_INSTANCE = 0
                        Try
                            nPROCESS_INSTANCE = CInt(dr("nPROCESS_INSTANCE"))
                        Catch ex As Exception
                        End Try

                        sKey = sPLANT & "." & sINV_ITEM_ID & "." & nRecId.ToString.PadLeft(4, "0"c)

                        ' build row values that we'll compare when updating this record later on
                        arrKeyValue = New Hashtable

                        Dim sFldKey As String = ""
                        Dim sFldVal As String = ""

                        For Each row1 As System.Data.DataRow In dr.GetSchemaTable.Rows
                            sFldKey = CStr(row1("ColumnName"))
                            sFldVal = "= ''"
                            If (sUpdateKeyColumns.IndexOf(sFldKey) > -1) Then
                                If (dr(sFldKey) Is DBNull.Value) Then
                                    sFldVal = "IS NULL"
                                Else
                                    Select Case row1("DataType")
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
                                                'sFldVal = "= '" & CStr(dr(sFldKey)).Trim & "'"
                                                sFldVal = "= '" & CStr(dr(sFldKey)) & "'"
                                            End If
                                    End Select
                                End If
                                If (Not arrKeyValue.ContainsKey(sFldKey)) Then
                                    arrKeyValue.Add(sFldKey, sFldVal)
                                End If
                            End If
                        Next

                        ' ready to process this NEW part/item
                        Try
                            '
                            ' (1) retrieve/request IOH data from customer
                            '
                            Dim nStartPos As Integer = 1
                            iBatch = ""

                            sr = New System.IO.StreamReader(CStr(My.Settings("MATMST_IOH_HTTP_SOAP_REQ")))
                            s = sr.ReadToEnd
                            sr.Dispose()
                            sr = Nothing

                            ' fields
                            '   Username                        {0}
                            '   Password                        {1}
                            '   NUMBER_OF_ROWS_FIRST_RETURNED   {2}
                            '   CURSOR_POSITION                 {3}
                            '   BIS_STORE                       {4}
                            '   BIS_PART                        {5}
                            sb = New System.Text.StringBuilder
                            sb.AppendFormat(s, oParent.Username, oParent.Password, nItemPerBatch.ToString, nStartPos.ToString, sPLANT, sINV_ITEM_ID)
                            iBatch = sb.ToString
                            sb = Nothing

                            'logger.WriteVerboseLog("loaded MATMST_IOH HTTP SOAP request XML : " & s)
                            logger.WriteVerboseLog("loaded MATMST_IOH HTTP SOAP request XML : " & CStr(My.Settings("MATMST_IOH_HTTP_SOAP_REQ")))

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

                            '
                            ' (2) process data retrieved from customer system
                            '
                            Dim arrMap As New System.Collections.Hashtable
                            Dim arrTable As New System.Collections.Hashtable

                            Dim xmlTableMap As New Xml.XmlDocument
                            xmlTableMap.Load(filename:=CStr(My.Settings("MATMST_IOH_HTTP_SOAP_DataMap")))
                            Dim ndTableMap As Xml.XmlNode = xmlTableMap.SelectSingleNode("tableColumnMappings")
                            If (ndTableMap.ChildNodes.Count > 0) Then
                                For Each ndRow As Xml.XmlNode In ndTableMap.ChildNodes
                                    If (ndRow.Name.Trim = "column") Then
                                        s = ""
                                        If (ndRow.Attributes("inColumnName").InnerText.Trim.Length > 0) Then
                                            ' use this field as key since we're going to search by this later on
                                            s = ndRow.Attributes("inColumnName").InnerText.Trim
                                        Else
                                            ' combine the "Id" and "value" as key
                                            s = ndRow.Attributes("Id").InnerText.Trim & "-" & _
                                                   ndRow.Attributes("value").InnerText.Trim
                                        End If
                                        arrMap.Add(key:=s, value:=New String() {ndRow.Attributes("targetTableName").InnerText.Trim, _
                                                                                ndRow.Attributes("targetColumnName").InnerText.Trim, _
                                                                                ndRow.Attributes("value").InnerText.Trim, _
                                                                                ndRow.Attributes("targetColumnLength").InnerText.Trim, _
                                                                                ndRow.Attributes("targetColumnDataType").InnerText.Trim})
                                    End If
                                Next
                            End If
                            xmlTableMap = Nothing

                            logger.WriteVerboseLog("loaded data map : " & CStr(My.Settings("MATMST_IOH_HTTP_SOAP_DataMap")))

                            Dim xmlIn As New System.Xml.XmlDocument

                            xmlIn.LoadXml(xml:=sHttpResponse)

                            Dim nsmgr As New Xml.XmlNamespaceManager(xmlIn.NameTable)
                            nsmgr.AddNamespace("resp", "http://schemas.datastream.net/MP_functions/MP0116_GetGridDataOnly_001_Result")

                            Dim nd As Xml.XmlNode = xmlIn.SelectSingleNode("//resp:GRIDRESULT", nsmgr)

                            ' (2.1) prepare fields
                            Dim sId As String = ""
                            Dim sName As String = ""
                            Dim sLabel As String = ""
                            Dim sTableName As String = ""
                            Dim sColumnName As String = ""
                            Dim sValue As String = ""
                            Dim nLen As Integer = 0
                            Dim sDataType As String = ""

                            Dim dtMap As New System.Data.DataTable
                            Dim ndField As Xml.XmlNode = nd.SelectSingleNode("resp:GRID", nsmgr).SelectSingleNode("resp:FIELDS", nsmgr)

                            For Each ndRow As Xml.XmlNode In ndField.ChildNodes
                                If (ndRow.Name.Trim.ToUpper = "FIELD") Then
                                    sId = ndRow.Attributes("aliasnum").InnerText.Trim
                                    sName = ndRow.Attributes("name").InnerText.Trim
                                    sLabel = ndRow.Attributes("label").InnerText.Trim
                                    ' only add those fields from customer that we have a mapping for (known ONLY)
                                    If arrMap.ContainsKey(sName.ToUpper) Then
                                        sTableName = CType(arrMap(sName.ToUpper), String())(0)          ' targetTableName
                                        sColumnName = CType(arrMap(sName.ToUpper), String())(1)         ' targetColumnName
                                        sValue = CType(arrMap(sName.ToUpper), String())(2)              ' targetValue
                                        nLen = CInt(CType(arrMap(sName.ToUpper), String())(3))          ' targetColumnLength
                                        sDataType = CType(arrMap(sName.ToUpper), String())(4)           ' targetColumnDataType
                                        dtMap.Columns.Add(New SDI.HTTP_SOAP_INTFC.CColumn(columnId:=sId, _
                                                                                          columnName:=sName, _
                                                                                          columnLabel:=sLabel, _
                                                                                          targetTableName:=sTableName, _
                                                                                          targetColumnName:=sColumnName, _
                                                                                          targetColumnDataType:=sDataType, _
                                                                                          targetColumnLength:=nLen, _
                                                                                          targetValue:=sValue))
                                    End If
                                End If
                            Next

                            For Each s3 As String In arrMap.Keys
                                If Not dtMap.Columns.Contains(name:=s3) Then
                                    dtMap.Columns.Add(New SDI.HTTP_SOAP_INTFC.CColumn(columnId:="", _
                                                                                      columnName:=s3, _
                                                                                      columnLabel:="", _
                                                                                      targetTableName:=CType(arrMap(s3), String())(0), _
                                                                                      targetColumnName:=CType(arrMap(s3), String())(1), _
                                                                                      targetColumnDataType:=CType(arrMap(s3), String())(4), _
                                                                                      targetColumnLength:=CInt(CType(arrMap(s3), String())(3)), _
                                                                                      targetValue:=CType(arrMap(s3), String())(2)))
                                End If
                            Next

                            For Each col As SDI.HTTP_SOAP_INTFC.CColumn In dtMap.Columns
                                ' grab unique tablename
                                If Not arrTable.ContainsKey(col.TargetTableName) Then
                                    arrTable.Add(col.TargetTableName, col.TargetTableName)
                                End If
                                ' set data type for each column
                                Select Case col.TargetColumnDataType.Trim.ToUpper
                                    Case "DECIMAL"
                                        col.DataType = GetType(System.Decimal)
                                    Case "DATETIME"
                                        col.DataType = GetType(System.DateTime)
                                    Case Else
                                        col.DataType = GetType(System.String)
                                End Select
                            Next

                            logger.WriteVerboseLog("matched column(s) with feed.")
                            logger.WriteVerboseLog("processing row(s).")

                            ' (2.2) look at ALL data/rows returned
                            Dim xOraTran1 As OleDb.OleDbTransaction = Nothing
                            Dim oraCN_Update As New OleDb.OleDbConnection(oraCNString)

                            oraCN_Update.Open()

                            If (oraCN_Update.State = ConnectionState.Open) Then

                                ' create the transaction object
                                xOraTran1 = oraCN_Update.BeginTransaction

                                Dim cmdUpdate As OleDb.OleDbCommand = Nothing
                                Dim idxCol As Integer = 0
                                Dim rowId As Integer = 0
                                Dim row As System.Data.DataRow = Nothing
                                Dim xLen As Integer = 0
                                Dim ndData As Xml.XmlNode = nd.SelectSingleNode("resp:GRID", nsmgr).SelectSingleNode("resp:DATA", nsmgr)

                                ' do we have any data from customer response?
                                '   if we do, save all IOH information
                                '   if WE DON'T, we'll just skip BUT will still need to flag that we've processed (or tried to look for IOH info) this part #/item
                                If Not (ndData Is Nothing) Then
                                    '
                                    ' (2.2.1) save all data rows returned since this pertains to a given part#/item
                                    '
                                    For Each ndRow As Xml.XmlNode In ndData.ChildNodes
                                        If (ndRow.Name.Trim.ToUpper = "ROW") Then

                                            ' retrieve each row
                                            rowId = CInt(ndRow.Attributes("id").InnerText)

                                            row = dtMap.NewRow

                                            For Each ndFld As Xml.XmlNode In ndRow.ChildNodes
                                                ' field Id
                                                s = ndFld.Attributes("n").InnerText
                                                idxCol = -1
                                                xLen = 0
                                                ' get index of column on our datatable
                                                For i As Integer = 0 To (dtMap.Columns.Count - 1)
                                                    If (CType(dtMap.Columns(i), SDI.HTTP_SOAP_INTFC.CColumn).ColumnId = s) Then
                                                        idxCol = i
                                                        xLen = CType(dtMap.Columns(i), SDI.HTTP_SOAP_INTFC.CColumn).TargetColumnLength
                                                        Exit For
                                                    End If
                                                Next
                                                If (idxCol >= 0) Then
                                                    Try
                                                        Select Case dtMap.Columns(idxCol).DataType
                                                            Case GetType(System.Decimal)
                                                                If (ndFld.InnerText.Trim.Length > 0) Then
                                                                    row(idxCol) = CDec(ndFld.InnerText)
                                                                Else
                                                                    row(idxCol) = 0
                                                                End If
                                                            Case GetType(System.DateTime)
                                                                If (ndFld.InnerText.Trim.Length > 0) Then
                                                                    row(idxCol) = CDate(ndFld.InnerText)
                                                                Else
                                                                    row(idxCol) = CDate("1/1/1900")
                                                                End If
                                                            Case Else
                                                                row(idxCol) = ndFld.InnerText.Replace("'"c, " "c)
                                                                If (xLen > 0) And (xLen < CStr(row(idxCol)).Length) Then
                                                                    row(idxCol) = CStr(row(idxCol)).Substring(0, xLen)
                                                                End If
                                                        End Select
                                                    Catch ex As Exception
                                                        Throw New ApplicationException(ex.ToString)
                                                    End Try
                                                End If
                                            Next ' For Each ndFld As Xml.XmlNode In ndRow.ChildNodes

                                            ' save each row (IOH record on target table/s)
                                            '   but we'll have to loop on however many defined target backend table
                                            For Each sKeyTableName As String In arrTable.Keys

                                                sql = "INSERT INTO SYSADM.{0} ({1}) VALUES ({2})"

                                                Dim columnList As String = ""
                                                Dim valueList As String = ""

                                                For Each col As SDI.HTTP_SOAP_INTFC.CColumn In dtMap.Columns
                                                    'col.DataType
                                                    If (col.TargetTableName = sKeyTableName) Then
                                                        ' column name
                                                        columnList &= col.TargetColumnName & ","
                                                        ' column value
                                                        If (col.TargetValue.Trim.Length > 0) Then
                                                            ' this means that value of this column, this process should supply it
                                                            If appVar.Vars.ContainsKey(col.TargetValue.Trim) Then
                                                                ' special types
                                                                If (col.TargetValue.Trim = "{SYSTEM:DATETIME}") Then
                                                                    valueList &= "" & appVar.varValue(col.TargetValue.Trim) & ","
                                                                Else
                                                                    Select Case appVar.varDataType(col.TargetValue.Trim).ToUpper
                                                                        Case "DATETIME"
                                                                            valueList &= "TO_DATE('" & CDate(appVar.varValue(col.TargetValue.Trim)).ToString("MM/dd/yyyy HH:mm:ss") & "','MM/DD/YYYY HH24:MI:SS'),"
                                                                        Case "DECIMAL"
                                                                            valueList &= "'" & CDec(appVar.varValue(col.TargetValue.Trim)).ToString & "',"
                                                                        Case Else
                                                                            valueList &= "'" & appVar.varValue(col.TargetValue.Trim) & "',"
                                                                    End Select
                                                                End If
                                                            Else
                                                                valueList &= "'',"
                                                            End If
                                                        Else
                                                            ' this column's value is coming out of the feed
                                                            Select Case col.DataType
                                                                Case GetType(System.DateTime)
                                                                    ' date/time
                                                                    If (row(col.ColumnName) Is System.DBNull.Value) Then
                                                                        valueList &= "TO_DATE('01/01/1900 00:00:00','MM/DD/YYYY HH24:MI:SS'),"
                                                                    Else
                                                                        valueList &= "TO_DATE('" & CDate(row(col.ColumnName)).ToString("MM/dd/yyyy HH:mm:ss") & "','MM/DD/YYYY HH24:MI:SS'),"
                                                                    End If
                                                                Case GetType(System.Decimal)
                                                                    ' decimal
                                                                    If (row(col.ColumnName) Is System.DBNull.Value) Then
                                                                        valueList &= "'0',"
                                                                    Else
                                                                        valueList &= "'" & CDec(row(col.ColumnName)).ToString & "',"
                                                                    End If
                                                                Case Else
                                                                    ' string
                                                                    If (row(col.ColumnName) Is System.DBNull.Value) Then
                                                                        valueList &= "'',"
                                                                    Else
                                                                        valueList &= "'" & CStr(row(col.ColumnName)) & "',"
                                                                    End If
                                                            End Select
                                                        End If
                                                    End If
                                                Next

                                                columnList = columnList.TrimEnd(","c)
                                                valueList = valueList.TrimEnd(","c)
                                                valueList = valueList.Replace("''", "' '")          ' single space for NULL values

                                                sb = New System.Text.StringBuilder
                                                sb.AppendFormat(sql, sKeyTableName, columnList, valueList)
                                                s = sb.ToString
                                                sb = Nothing

                                                cmdUpdate = oraCN_Update.CreateCommand
                                                cmdUpdate.CommandText = s
                                                cmdUpdate.CommandType = CommandType.Text
                                                cmdUpdate.Transaction = xOraTran1
                                                Try
                                                    cmdUpdate.ExecuteNonQuery()
                                                Catch ex As Exception
                                                    xOraTran1.Rollback()
                                                    xOraTran1.Dispose()
                                                    xOraTran1 = Nothing
                                                    Throw New ApplicationException(message:=ex.ToString)
                                                End Try
                                                Try
                                                    cmdUpdate.Dispose()
                                                Catch ex As Exception
                                                End Try
                                                cmdUpdate = Nothing

                                            Next

                                            ' dispose this row
                                            row = Nothing

                                        End If 'If (ndRow.Name.Trim.ToUpper = "ROW") Then
                                    Next 'For Each ndRow As Xml.XmlNode In ndData.ChildNodes

                                    ndData = Nothing
                                Else
                                    logger.WriteVerboseLog("NO IOH data returned :: " & sPLANT & " / " & sINV_ITEM_ID)
                                End If 'If Not (ndData Is Nothing) Then

                                '
                                '   (2.2.2) update material master table after processing IOH record/s
                                '
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

                                sr = New System.IO.StreamReader(CStr(My.Settings("MATMST_IOH_FlagMatMast")))
                                s = sr.ReadToEnd
                                sr.Dispose()
                                sr = Nothing

                                ' fields 
                                '   PROCESS_INSTANCE            {0}
                                '   "WHERE clause"              {1}
                                sb = New System.Text.StringBuilder
                                sb.AppendFormat(s, processInstanceAfterProcess.ToString, sWhere)
                                s = sb.ToString
                                sb = Nothing

                                cmdUpdate = oraCN_Update.CreateCommand
                                cmdUpdate.CommandText = s
                                cmdUpdate.CommandType = CommandType.Text
                                cmdUpdate.Transaction = xOraTran1
                                Try
                                    cmdUpdate.ExecuteNonQuery()
                                Catch ex As Exception
                                    xOraTran1.Rollback()
                                    xOraTran1.Dispose()
                                    xOraTran1 = Nothing
                                    Throw New ApplicationException(message:=ex.ToString)
                                End Try
                                Try
                                    cmdUpdate.Dispose()
                                Catch ex As Exception
                                End Try
                                cmdUpdate = Nothing

                                ' commit transaction
                                '   and dispose object
                                xOraTran1.Commit()
                                xOraTran1.Dispose()
                                xOraTran1 = Nothing

                            Else
                                ' cannot open connection (which will be weird since it's the same connection for reading data from material master table) BUT just in case
                                '   just by-pass this material master record and response for IOH information from customer .. just log
                                logger.WriteErrorLog("CANNOT open connection for writing IOH record information and for updating material master record - WEIRD :: " & sPLANT & " / " & sINV_ITEM_ID)
                            End If 'If (oraCN_Update.State = ConnectionState.Open) Then

                            ' clean-up
                            Try
                                oraCN_Update.Close()
                            Catch ex As Exception
                            End Try
                            Try
                                oraCN_Update.Dispose()
                            Catch ex As Exception
                            End Try
                            oraCN_Update = Nothing

                            ndField = Nothing
                            dtMap = Nothing

                            nd = Nothing
                            nsmgr = Nothing
                            xmlIn = Nothing
                            xmlTableMap = Nothing

                            arrTable = Nothing
                            arrMap = Nothing

                        Catch ex As Exception
                            ' just ignore and process the next item
                            '       log this error though
                            logger.WriteErrorLog("ERROR :: " & ex.ToString)
                        End Try

                    End While

                    ' clean-up
                    arrKeyValue = Nothing

                Else 'If dr.HasRows Then
                    logger.WriteInformationLog("no NEW material master part/item to process.")
                End If 'If dr.HasRows Then
            End If 'If Not (dr Is Nothing) Then

            Try
                dr.Close()
            Catch ex As Exception
            End Try
            dr = Nothing

            Try
                cmdRead.Dispose()
            Catch ex As Exception
            End Try
            cmdRead = Nothing

        Else
            ' unable to open connection!
            logger.WriteErrorLog("DB : unable to open connection : " & oraCNString)
        End If

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
        appVar = Nothing
        emailer = Nothing
        logger = Nothing
        oParent = Nothing

    End Sub

End Module
