Module IOH

    Sub Main()

        Const processCode As String = "IOH"

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
            s1 = My.Settings(propertyName:="IOH_GetBatchSize")
        Catch ex As Exception
            s1 = "0"
        End Try
        If CInt(s1) > 0 Then
            nItemPerBatch = CInt(s1)
        End If
        Try
            s1 = My.Settings(propertyName:="IOH_MaxReq")
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

        Dim cmd As OleDb.OleDbCommand = Nothing
        Dim sb As System.Text.StringBuilder = Nothing
        Dim sql As String = ""
        Dim oBatch As New IOH_batch
        Dim oraCN1 As New OleDb.OleDbConnection(oraCNString)

        oraCN1.Open()

        If (oraCN1.State = ConnectionState.Open) Then

            logger.WriteVerboseLog("using DB : " & oraCN1.Database)

            ' prepare transaction
            Dim oraTran1 As OleDb.OleDbTransaction = oraCN1.BeginTransaction
            logger.WriteVerboseLog("DB connection opened and transaction created")

            ' remove existing records (if any) in preparation for new set from customer
            sql = "" & _
                  "DELETE FROM SYSADM.PS_ISA_MB_IOH_WS " & _
                  "WHERE CUST_ID = '{0}' " & _
                  ""
            sb = New System.Text.StringBuilder
            sb.AppendFormat(sql, custId)
            cmd = oraCN1.CreateCommand
            cmd.CommandText = sb.ToString
            cmd.CommandType = CommandType.Text
            cmd.Transaction = oraTran1
            cmd.ExecuteNonQuery()
            cmd.Dispose()
            cmd = Nothing
            sb = Nothing
            logger.WriteVerboseLog("removed existing records")

            nTotalRecordsProcessed = 0

            ' retrieve records from customer's system - batch
            While (nLoopCtr < nMaxReq)

                ' increment loop counter
                '   will also be used as cursor position index
                nLoopCtr += 1

                ' prepare batch
                oBatch.index = (nTotalRecordsProcessed + 1)
                oBatch.BatchSize = nItemPerBatch
                oBatch.ex = Nothing

                ' retrieve, save and check batch result
                Try
                    logger.WriteVerboseLog("#" & nLoopCtr.ToString & " batch")
                    RetrieveAndProcess(oraCN1, custId, processId, oraTran1, oBatch, appVar, oParent)
                    nTotalRecordsProcessed += oBatch.BatchProcessedRecordCount
                Catch ex As Exception
                    oBatch.ex = ex
                    logger.WriteErrorLog("batch error : " & ex.ToString)
                    logger.WriteErrorLog("error occured on : " & (nTotalRecordsProcessed + oBatch.BatchProcessedRecordCount).ToString)
                End Try

                If Not (oBatch.ex Is Nothing) Or _
                  (oBatch.BatchProcessedRecordCount < oBatch.BatchSize) Then
                    ' exit loop if either (1) there's an error or (2) total processed record count is less than requested - EOF
                    Exit While
                Else
                    ' reset batch counter
                    oBatch.BatchProcessedRecordCount = 0
                End If

            End While

            ' rollback or commit
            If Not (oBatch.ex Is Nothing) Then
                oraTran1.Rollback()
                logger.WriteInformationLog("DB : ROLLED BACK changes")
            Else
                oraTran1.Commit()
                logger.WriteInformationLog("DB : COMMITTED changes")
                logger.WriteInformationLog("processed total # of record(s) : " & nTotalRecordsProcessed.ToString)
            End If

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

    Private Sub RetrieveAndProcess(ByVal oraCN As OleDb.OleDbConnection, _
                                   ByVal customerId As String, _
                                   ByVal processId As String, _
                                   ByVal oraTran As OleDb.OleDbTransaction, _
                                   ByVal oBatch As IOH_batch, _
                                   ByVal vars As SDI.HTTP_SOAP_INTFC.appVarCollection, _
                                   ByVal oParent As SDI.HTTP_SOAP_INTFC.IParentApp)

        Dim logger As SDI.ApplicationLogger.IApplicationLogger = oParent.Logger
        Dim emailer As SDI.EmailNotifier.IEmailNotifier = oParent.EmailNotifier

        Dim sb As System.Text.StringBuilder = Nothing
        Dim Response_Doc As String = Nothing

        Dim idx As String = oBatch.index.ToString.PadLeft(oBatch.BatchSize.ToString.Length, "0"c)
        'Dim fle As String = "c:\tmp\" & processId & " (" & idx & ").xml"
        Dim flePath As String = oParent.TemporaryFilePath.Trim
        While (flePath.Length > 0) And (flePath.LastIndexOf("\"c) = (flePath.Length - 1))
            flePath = flePath.TrimEnd("\"c)
        End While
        Dim fle As String = ""
        Dim sw As System.IO.StreamWriter = Nothing

        '
        ' (1) retrieve data from customer
        '

        'Dim sr As New System.IO.StreamReader("C:\projects\Apps\MOM Brands - EAM integration\IOH_HTTP_SOAP\IOH_HTTP_SOAP\MOMBrand_IOH_HTTP_SOAP_Request.xml")
        Dim sr As New System.IO.StreamReader(CStr(My.Settings("IOH_HTTP_SOAP_REQ")))
        Dim s As String = sr.ReadToEnd
        sr.Dispose()
        sr = Nothing

        sb = New System.Text.StringBuilder
        sb.AppendFormat(s, oParent.Username, oParent.Password, oBatch.BatchSize.ToString, oBatch.index.ToString)

        'logger.WriteVerboseLog("loaded IOH HTTP SOAP request XML : " & s)
        logger.WriteVerboseLog("loaded IOH HTTP SOAP request XML : " & CStr(My.Settings("IOH_HTTP_SOAP_REQ")))

        Dim httpSession As New SDI.HTTP_SOAP_INTFC.easyHttp

        s = SDI.HTTP_SOAP_INTFC.Common.RemoveCrLf(sb.ToString)

        ' only create this file (request) if logging level is verbose
        If logger.LoggingLevel = TraceLevel.Verbose Then
            fle = flePath & "\" & processId & " (" & idx & ")-req.xml"
            sw = New System.IO.StreamWriter(path:=fle, append:=False)
            sw.WriteLine(s)
            sw.Flush()
            sw.Close()
            sw.Dispose()
            sw = Nothing
        End If

        'httpSession.URL = "https://eam.saas.infor.com/EAM85WS/axis/services/EWSConnector"
        httpSession.URL = oParent.TargetURL
        httpSession.DataToPost = s
        httpSession.ContentType = "text/xml; charset=utf-8"
        httpSession.Method = SDI.HTTP_SOAP_INTFC.easyHttp.HTTPMethod.HTTP_POST
        httpSession.IgnoreServerCertificate = False
        httpSession.HeaderAttributes.Add(name:="SOAPAction", value:="https://schemas.microsoft.com/crm/2006/WebServices/Retrieve")

        Response_Doc = ""

        Try
            'Response_Doc = httpSession.SendAsBytes
            Response_Doc = httpSession.SendAsString
        Catch ex As Exception
            ''Response.Write(ex.ToString)
            'ShowErrorMessage(ex.ToString)
            Response_Doc &= ex.ToString
        End Try

        httpSession = Nothing
        sb = Nothing

        ' clean the response string
        '   "System.Xml.XmlException: The XML declaration is unexpected ..." error message due to unexpected char before the xml declaration!?
        Response_Doc = SDI.HTTP_SOAP_INTFC.Common.RemoveCrLf(Response_Doc)

        fle = flePath & "\" & processId & " (" & idx & ")-resp.xml"
        sw = New System.IO.StreamWriter(path:=fle, append:=False)
        sw.WriteLine(Response_Doc)
        sw.Flush()
        sw.Close()
        sw.Dispose()
        sw = Nothing

        logger.WriteVerboseLog("response XML file : " & fle)

        '
        ' (2) process data retrieved from customer system
        '

        Dim arrMap As New System.Collections.Hashtable
        Dim arrTable As New System.Collections.Hashtable

        Dim xmlTableMap As New Xml.XmlDocument
        'xmlTableMap.Load(filename:="C:\projects\Apps\MOM Brands - EAM integration\IOH_HTTP_SOAP\IOH_HTTP_SOAP\MOMBrand_IOH_HTTP_SOAP_TableColMap.xml")
        xmlTableMap.Load(filename:=CStr(My.Settings("IOH_HTTP_SOAP_DataMap")))
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

        logger.WriteVerboseLog("loaded data map : " & CStr(My.Settings("IOH_HTTP_SOAP_DataMap")))

        Dim sql As String = ""
        Dim cmd As OleDb.OleDbCommand = Nothing

        Dim xmlIn As New System.Xml.XmlDocument

        xmlIn.LoadXml(xml:=Response_Doc)

        Dim nsmgr As New Xml.XmlNamespaceManager(xmlIn.NameTable)
        nsmgr.AddNamespace("resp", "http://schemas.datastream.net/MP_functions/MP0116_GetGridDataOnly_001_Result")

        Dim nd As Xml.XmlNode = xmlIn.SelectSingleNode("//resp:GRIDRESULT", nsmgr)

        '   (1) fields
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

        For Each s1 As String In arrMap.Keys
            If Not dtMap.Columns.Contains(name:=s1) Then
                dtMap.Columns.Add(New SDI.HTTP_SOAP_INTFC.CColumn(columnId:="", _
                                                                  columnName:=s1, _
                                                                  columnLabel:="", _
                                                                  targetTableName:=CType(arrMap(s1), String())(0), _
                                                                  targetColumnName:=CType(arrMap(s1), String())(1), _
                                                                  targetColumnDataType:=CType(arrMap(s1), String())(4), _
                                                                  targetColumnLength:=CInt(CType(arrMap(s1), String())(3)), _
                                                                  targetValue:=CType(arrMap(s1), String())(2)))
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

        '   (2) data/rows
        Dim idxCol As Integer = 0
        Dim rowId As Integer = 0
        Dim row As System.Data.DataRow = Nothing
        Dim xLen As Integer = 0
        Dim ndData As Xml.XmlNode = nd.SelectSingleNode("resp:GRID", nsmgr).SelectSingleNode("resp:DATA", nsmgr)

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
                Next

                ' save each row 
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
                                If vars.Vars.ContainsKey(col.TargetValue.Trim) Then
                                    ' special types
                                    If (col.TargetValue.Trim = "{SYSTEM:DATETIME}") Then
                                        valueList &= "" & vars.varValue(col.TargetValue.Trim) & ","
                                    Else
                                        Select Case vars.varDataType(col.TargetValue.Trim).ToUpper
                                            Case "DATETIME"
                                                valueList &= "TO_DATE('" & CDate(vars.varValue(col.TargetValue.Trim)).ToString("MM/dd/yyyy HH:mm:ss") & "','MM/DD/YYYY HH24:MI:SS'),"
                                            Case "DECIMAL"
                                                valueList &= "'" & CDec(vars.varValue(col.TargetValue.Trim)).ToString & "',"
                                            Case Else
                                                valueList &= "'" & vars.varValue(col.TargetValue.Trim) & "',"
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

                    cmd = oraCN.CreateCommand
                    cmd.CommandText = sb.ToString
                    cmd.CommandType = CommandType.Text
                    cmd.Transaction = oraTran
                    Try
                        cmd.ExecuteNonQuery()
                    Catch ex As Exception
                        Throw New ApplicationException(message:=ex.ToString)
                    End Try
                    cmd.Dispose()
                    cmd = Nothing

                    sb = Nothing

                Next

                ' increment processed record counter
                oBatch.BatchProcessedRecordCount += 1

                ' dispose this row
                row = Nothing

            End If
        Next

        logger.WriteVerboseLog("[batch] total # of row(s) process : " & oBatch.BatchProcessedRecordCount.ToString)

        ndData = Nothing
        ndField = Nothing
        nd = Nothing

        dtMap.Dispose()
        dtMap = Nothing

        nsmgr = Nothing
        xmlIn = Nothing

        arrTable = Nothing
        arrMap = Nothing

        emailer = Nothing
        logger = Nothing

    End Sub

End Module
