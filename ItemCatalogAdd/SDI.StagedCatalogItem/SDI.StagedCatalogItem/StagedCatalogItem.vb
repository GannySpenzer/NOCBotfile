Imports System.Data.OleDb
Imports System.Data.SqlClient

Module StagedCatalogItem

    Sub Main()

        Dim logger As SDI.ApplicationLogger.appLogger = Nothing
        Dim logPath As String = ""
        Dim logFile As String = ""
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        Dim dStart As DateTime = Now

        ' this app name/version
        Dim meName As String = System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName
        Dim meVer As String = System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString

        Dim sLastRunInfoPath As String = System.IO.Path.GetDirectoryName(meName).Trim.TrimEnd("\"c)
        Dim sLastRunInfoFile As String = sLastRunInfoPath & "\" & CStr(My.Settings("LastRunInfoXML"))

        ' logging
        Dim s As String = ""
        Try
            s = CStr(My.Settings(propertyName:="appLogLevel")).Trim.ToUpper
        Catch ex As Exception
            s = ""
        End Try
        If (s.Length > 0) Then
            If (s.IndexOf("VERB") > -1) Then
                logLevel = TraceLevel.Verbose
            ElseIf (s.IndexOf("INFO") > -1) Or (s.IndexOf("INFORMATION") > -1) Then
                logLevel = TraceLevel.Info
            ElseIf (s.IndexOf("WARNING") > -1) Or (s.IndexOf("WARN") > -1) Then
                logLevel = TraceLevel.Warning
            ElseIf (s.IndexOf("ERROR") > -1) Then
                logLevel = TraceLevel.Error
            ElseIf (s.IndexOf("OFF") > -1) Then
                logLevel = TraceLevel.Off
            Else
                ' don't change default
            End If
        End If

        ' log path
        '   default to location of executing assembly on \Logs folder
        logPath = meName.Substring(0, meName.LastIndexOf("\"c)) & "\Logs"
        's = ""
        'Try
        '    s = CStr(My.Settings(propertyName:="appLogPath")).Trim
        'Catch ex As Exception
        '    s = ""
        'End Try
        'If (s.Length > 0) Then
        '    logPath = s
        'End If
        While (logPath.Length > 0) And (logPath.Trim.LastIndexOf("\"c) = (logPath.Trim.Length - 1))
            logPath = logPath.Trim.TrimEnd("\"c)
        End While
        If Not System.IO.Directory.Exists(logPath) Then
            System.IO.Directory.CreateDirectory(logPath)
        End If

        ' log filename ID
        Dim sLogFilenameId As String = "SDI.StagedCatalogItem"
        's = ""
        'Try
        '    s = CStr(My.Settings(propertyName:="appLogFilenameId")).Trim
        'Catch ex As Exception
        '    s = ""
        'End Try
        'If (s.Length > 0) Then
        '    sLogFilenameId = s
        'End If

        ' log file
        logFile = logPath & "\" & sLogFilenameId & "_" & Now.ToString("yyyyMMdd") & Now.GetHashCode.ToString & ".log"

        ' logger
        logger = New SDI.ApplicationLogger.appLogger(logFile, logLevel)
        logger.WriteInformationLog("starting " & meName & " v" & meVer)

        ' for remembering when/what we last did this
        Dim xmlLastRun As New System.Xml.XmlDocument
        xmlLastRun.Load(filename:=sLastRunInfoFile)

        Dim nsmgr As System.Xml.XmlNamespaceManager = New System.Xml.XmlNamespaceManager(xmlLastRun.NameTable)
        nsmgr.AddNamespace("config", "mainConfiguration")

        Dim nd As System.Xml.XmlNode = Nothing
        nd = xmlLastRun.SelectSingleNode("//config:LastSuccessfulDateTimeRun", nsmgr)

        Dim dtLookUpStart As DateTime = CDate(nd.InnerText)
        Dim dtLookUpEnd As DateTime = Now
        logger.WriteVerboseLog("date range : " & dtLookUpStart.ToString("MM/dd/yyyy HH:mm:ss") & " to " & dtLookUpEnd.ToString("MM/dd/yyyy HH:mm:ss"))

        Dim sql As String = ""
        Dim sb As System.Text.StringBuilder = Nothing
        Dim sr As System.IO.StreamReader = Nothing

        Dim oraCN As OleDbConnection = Nothing

        '
        ' build catalog/product view cross reference list
        '
        Dim sqlCPlusCN As New SqlConnection(CStr(My.Settings("sqlCPlusCNString")))
        sqlCPlusCN.Open()
        sr = New System.IO.StreamReader(CStr(My.Settings("qryGetCatalogProductViewList")).Trim)
        sql = sr.ReadToEnd
        sr.Dispose()
        sr = Nothing
        Dim oCatalogProductViews As CatalogProductViews = New CatalogProductViews(sqlCPlusCN, sql)
        Try
            sqlCPlusCN.Close()
        Catch ex As Exception
        End Try
        Try
            sqlCPlusCN.Dispose()
        Catch ex As Exception
        End Try
        sqlCPlusCN = Nothing
        logger.WriteVerboseLog("catalog product views (count) : " & oCatalogProductViews.Item.Count.ToString)

        '
        ' build site/product view cross reference list
        '
        oraCN = New OleDbConnection(connectionString:=CStr(My.Settings("oraCNString")))
        oraCN.Open()
        sr = New System.IO.StreamReader(CStr(My.Settings("qryGetSiteProductViewList")).Trim)
        sql = sr.ReadToEnd
        sr.Dispose()
        sr = Nothing
        Dim oSiteProductViews As SiteProductViews = New SiteProductViews(oraCN, sql)
        Try
            oraCN.Close()
        Catch ex As Exception
        End Try
        Try
            oraCN.Dispose()
        Catch ex As Exception
        End Try
        oraCN = Nothing
        logger.WriteVerboseLog("site/product view cross reference (count) : " & oSiteProductViews.Item.Count.ToString)

        Dim cnString As String = CStr(My.Settings("sql2012CNString")).Replace("{0}", "SDI_CPlus_Extend")
        Dim sql2012CN As New SqlConnection(cnString)

        '
        ' build site/item prefix cross reference list
        '
        sql2012CN.Open()
        sr = New System.IO.StreamReader(CStr(My.Settings("qryGetSiteItemPrefixList")).Trim)
        sql = sr.ReadToEnd
        sr.Dispose()
        sr = Nothing
        Dim oSiteItemPrefixes As SiteItemPrefixes = New SiteItemPrefixes(sql2012CN, sql)
        Try
            sql2012CN.Close()
        Catch ex As Exception
        End Try
        Try
            sql2012CN.Dispose()
        Catch ex As Exception
        End Try
        sql2012CN = Nothing
        logger.WriteVerboseLog("site/item prefix cross reference (count) : " & oSiteItemPrefixes.Item.Count.ToString)

        sql2012CN = New SqlConnection(cnString)
        'sql2012CN.Open()

        '
        ' grab item list from Oracle table/s
        '   for all Oracle access, we'll use Microsoft OleDB provider at the moment since we're not sure if target server might not have Oracle provider
        '
        oraCN = New OleDbConnection(connectionString:=CStr(My.Settings("oraCNString")))

        oraCN.Open()

        If (oraCN.State = ConnectionState.Open) Then

            sr = New System.IO.StreamReader(CStr(My.Settings("qryGetNewModPSItem")).Trim)
            sql = sr.ReadToEnd
            sr.Dispose()
            sr = Nothing

            ' fields 
            '   {0}         start d/t
            '   {1}         end d/t 
            sb = New System.Text.StringBuilder
            ' normal daily run
            sb.AppendFormat(sql, _
                            dtLookUpStart.ToString("MM/dd/yyyy HH:mm:ss"), _
                            dtLookUpEnd.ToString("MM/dd/yyyy HH:mm:ss"))
            '' for special run - ie., Ascend items or MOM Brand items ONLY
            'sb.AppendFormat(sql, dtLookUpEnd.ToString("MM/dd/yyyy HH:mm:ss"))
            sql = sb.ToString
            sb = Nothing

            Dim cmdGetItemList As OleDbCommand = oraCN.CreateCommand
            cmdGetItemList.CommandText = sql
            cmdGetItemList.CommandType = CommandType.Text

            Dim rdrItemList As OleDbDataReader = Nothing

            Try
                rdrItemList = cmdGetItemList.ExecuteReader
            Catch ex As Exception
            End Try

            If Not (rdrItemList Is Nothing) Then
                Dim boItem As item = Nothing

                While rdrItemList.Read

                    If Not (boItem Is Nothing) Then
                        boItem = Nothing
                    End If

                    ' item information
                    Dim sInvItemId As String = ""
                    Dim sItemPrefix As String = ""
                    Dim sCustomerItemId As String = ""
                    Dim dtEffective As DateTime
                    Dim sStatus As String = ""
                    Dim sItemType As String = ""
                    Dim sStockType As String = ""
                    Dim sItemDescription As String = ""
                    Dim nItemId As Integer = 0
                    Dim sUM As String = ""
                    Dim sMfg As String = ""
                    Dim sMfgPartNo As String = ""

                    Dim sSiteId As String = ""
                    Dim nCatalogItemId As Integer = 0

                    Try
                        sInvItemId = CStr(rdrItemList("INV_ITEM_ID")).Trim.ToUpper
                    Catch ex As Exception
                    End Try

                    Try
                        sItemPrefix = CStr(rdrItemList("ITEM_PREFIX")).Trim.ToUpper
                    Catch ex As Exception
                    End Try

                    Try
                        sCustomerItemId = CStr(rdrItemList("CUSTOMER_ITEM_ID")).Trim.ToUpper
                    Catch ex As Exception
                    End Try

                    Try
                        dtEffective = CDate(rdrItemList("EFF_DATE"))
                    Catch ex As Exception
                    End Try

                    Try
                        sStatus = CStr(rdrItemList("EFF_STATUS"))
                    Catch ex As Exception
                    End Try

                    Try
                        sItemType = CStr(rdrItemList("ITEM_TYPE"))
                    Catch ex As Exception
                    End Try

                    Try
                        sStockType = CStr(rdrItemList("STOCK_TYPE"))
                    Catch ex As Exception
                    End Try

                    Try
                        sItemDescription = CStr(rdrItemList("DESCR254"))
                    Catch ex As Exception
                    End Try

                    Try
                        nItemId = CInt(rdrItemList("ITEM_ID"))
                    Catch ex As Exception
                    End Try

                    Try
                        sUM = CStr(rdrItemList("UM"))
                    Catch ex As Exception
                    End Try

                    Try
                        sSiteId = CStr(rdrItemList("ISA_SITE_ID"))
                    Catch ex As Exception
                    End Try

                    Try
                        nCatalogItemId = CInt(rdrItemList("CATALOG_ITEM_ID"))
                    Catch ex As Exception
                    End Try

                    Try
                        sMfg = CStr(rdrItemList("MFG_ID")).Trim
                    Catch ex As Exception
                    End Try

                    Try
                        sMfgPartNo = CStr(rdrItemList("MFG_PART_NUMBER")).Trim
                    Catch ex As Exception
                    End Try

                    ' this item prefix MIGHT be associated with multiple site IDs
                    '   so we'll process this item for all of them (ie., COC for site 905,906,907 and 908, N17 for 230 and 260)
                    Dim oEnumerator As IDictionaryEnumerator = oSiteItemPrefixes.Item.GetEnumerator
                    While oEnumerator.MoveNext
                        Dim sKey As String = CStr(oEnumerator.Key)
                        If (sKey.IndexOf(sItemPrefix) > -1) Then

                            Dim sSiteIdForPrefix As String = CType(CType(oEnumerator.Value, String())(1), String)
                            Dim sIndicatorFlagForPrefix As String = CType(CType(oEnumerator.Value, String())(2), String)

                            ' at the time of this writing the following were the only indicator flag
                            '       "A" - Active and means that customerItemID won't have item prefix
                            '       "S" - Special and means that customerItemID is exactly same as INV_ITEM_ID (with prefix in it)
                            If (sIndicatorFlagForPrefix = "S") Then
                                sCustomerItemId = sInvItemId
                            End If

                            boItem = itemFactory.CreateItemInstance(sInvItemId, sStatus, sItemDescription, nItemId, sSiteId, nCatalogItemId, sUM, sMfg, sMfgPartNo, sCustomerItemId, sItemPrefix)

                            boItem.Catalog_ProductViewId = itemFactory.GetProductViewId(boItem, oCatalogProductViews, oSiteProductViews, sSiteIdForPrefix, sIndicatorFlagForPrefix)
                            boItem.CatalogId = itemFactory.GetItemCatalogId(boItem, oCatalogProductViews, oSiteProductViews, sSiteIdForPrefix, sIndicatorFlagForPrefix)
                            boItem.SiteId = itemFactory.GetSiteId(boItem, oCatalogProductViews, oSiteProductViews, sSiteIdForPrefix, sIndicatorFlagForPrefix)

                            ' process for each item
                            If (boItem.CatalogId > 0) And (boItem.Catalog_ProductViewId > 0) Then
                                logger.WriteVerboseLog("processing item : item=" & boItem.PS_ItemId & _
                                                       "; catalog=" & boItem.CatalogId & _
                                                       "; product view=" & boItem.Catalog_ProductViewId & _
                                                       "; catalog item id=" & boItem.Catalog_ItemId.ToString & _
                                                       "")
                                ' open sql connection
                                sql2012CN.Open()
                                ' process item
                                ProcessItem(boItem, oraCN, sql2012CN, logger)
                                ' close sql connection
                                Try
                                    sql2012CN.Close()
                                Catch ex As Exception
                                End Try
                            Else
                                logger.WriteVerboseLog("unable to process item : item=" & boItem.PS_ItemId & _
                                                       "; catalog=" & boItem.CatalogId & _
                                                       "; product view=" & boItem.Catalog_ProductViewId & _
                                                       "")
                            End If

                        End If
                    End While

                End While

                boItem = Nothing
            End If

            Try
                rdrItemList.Close()
            Catch ex As Exception
            End Try
            rdrItemList = Nothing

            Try
                cmdGetItemList.Dispose()
            Catch ex As Exception
            End Try
            cmdGetItemList = Nothing

        Else
            ' unable to open connection (oracle)
            logger.WriteErrorLog("unable to open connection : " & oraCN.DataSource)
        End If

        Try
            oraCN.Close()
            oraCN.Dispose()
        Catch ex As Exception
        Finally
            oraCN.Dispose()
        End Try
        oraCN = Nothing

        Try
            sql2012CN.Close()
            sql2012CN.Dispose()
        Catch ex As Exception
        End Try
        sql2012CN = Nothing

        sb = Nothing
        sr = Nothing

        '
        ' remember the last date/time we completely execute this process
        '       so we can use this date when we need to go back to Oracle to grab any new item(s) added
        '
        nd.InnerText = dtLookUpEnd.ToString("MM/dd/yyyy HH:mm:ss.fff")
        xmlLastRun.Save(filename:=sLastRunInfoFile)

        nd = Nothing
        nsmgr = Nothing
        xmlLastRun = Nothing

        logger.WriteInformationLog("finished executing " & meName)

        logLevel = Nothing
        logger = Nothing

    End Sub

    Private Sub ProcessItem(ByVal oItem As item, _
                            ByVal oraCN As OleDbConnection, _
                            ByVal sql2012CN As SqlConnection, _
                            ByVal logger As SDI.ApplicationLogger.IApplicationLogger)
        Dim sql As String = ""
        Dim sb As System.Text.StringBuilder = Nothing
        Dim sr As System.IO.StreamReader = Nothing

        Dim oraTran As OleDbTransaction = Nothing
        Dim sql2012Tran As SqlTransaction = Nothing

        Dim bIsNewItem As Boolean = IsNewItem(oItem, sql2012CN)

        If bIsNewItem Then
            logger.WriteVerboseLog(vbTab & "processing as NEW item")
            '
            ' add into ScottsdaleItemTable
            '   get back itemID value
            '
            Dim parmSql As SqlParameter = Nothing
            Dim cmdSql As SqlCommand = sql2012CN.CreateCommand

            cmdSql.CommandText = "_sdiCreateTempScottsdaleItemTable"
            cmdSql.CommandType = CommandType.StoredProcedure
            cmdSql.Transaction = sql2012Tran

            parmSql = cmdSql.CreateParameter
            parmSql.ParameterName = "@catalogId"
            parmSql.SqlDbType = SqlDbType.BigInt
            parmSql.Size = 0
            parmSql.Value = oItem.CatalogId
            parmSql.Direction = ParameterDirection.Input
            cmdSql.Parameters.Add(parmSql)
            parmSql = Nothing

            parmSql = cmdSql.CreateParameter
            parmSql.ParameterName = "@customerItemId"
            parmSql.SqlDbType = SqlDbType.NVarChar
            parmSql.Size = 100
            parmSql.Value = oItem.PS_ItemId
            parmSql.Direction = ParameterDirection.Input
            cmdSql.Parameters.Add(parmSql)
            parmSql = Nothing

            parmSql = cmdSql.CreateParameter
            parmSql.ParameterName = "@itemDescription"
            parmSql.SqlDbType = SqlDbType.NVarChar
            parmSql.Size = 100
            parmSql.Value = oItem.PS_ItemDescription
            parmSql.Direction = ParameterDirection.Input
            cmdSql.Parameters.Add(parmSql)
            parmSql = Nothing

            parmSql = cmdSql.CreateParameter
            parmSql.ParameterName = "@um"
            parmSql.SqlDbType = SqlDbType.NVarChar
            parmSql.Size = 12
            parmSql.Value = oItem.PS_UnitOfMeasure
            parmSql.Direction = ParameterDirection.Input
            cmdSql.Parameters.Add(parmSql)
            parmSql = Nothing

            Dim mfg As String = oItem.PS_ManufacturerId.Trim
            parmSql = cmdSql.CreateParameter
            parmSql.ParameterName = "@mfg"
            parmSql.SqlDbType = SqlDbType.NVarChar
            parmSql.Size = 100
            If (mfg.Length = 0) Then
                parmSql.Value = System.DBNull.Value
            Else
                parmSql.Value = mfg
            End If
            parmSql.Direction = ParameterDirection.Input
            cmdSql.Parameters.Add(parmSql)
            parmSql = Nothing

            Dim mfgPart As String = oItem.PS_ManufacturerPartNumber.Trim
            parmSql = cmdSql.CreateParameter
            parmSql.ParameterName = "@mfgPartNumber"
            parmSql.SqlDbType = SqlDbType.NVarChar
            parmSql.Size = 100
            If (mfgPart.Length = 0) Then
                parmSql.Value = System.DBNull.Value
            Else
                parmSql.Value = mfgPart
            End If
            parmSql.Direction = ParameterDirection.Input
            cmdSql.Parameters.Add(parmSql)
            parmSql = Nothing

            parmSql = cmdSql.CreateParameter
            parmSql.ParameterName = "@classId_OUT"
            parmSql.SqlDbType = SqlDbType.BigInt
            parmSql.Size = 0
            parmSql.Value = System.DBNull.Value
            parmSql.Direction = ParameterDirection.Output
            cmdSql.Parameters.Add(parmSql)
            parmSql = Nothing

            parmSql = cmdSql.CreateParameter
            parmSql.ParameterName = "@itemId_OUT"
            parmSql.SqlDbType = SqlDbType.BigInt
            parmSql.Size = 0
            parmSql.Value = System.DBNull.Value
            parmSql.Direction = ParameterDirection.Output
            cmdSql.Parameters.Add(parmSql)
            parmSql = Nothing

            Dim i As Integer = cmdSql.ExecuteNonQuery

            oItem.Catalog_ClassId = CInt(cmdSql.Parameters("@classId_OUT").Value)
            oItem.Catalog_ItemId = CInt(cmdSql.Parameters("@itemId_OUT").Value)

            Try
                cmdSql.Dispose()
            Catch ex As Exception
            End Try
            cmdSql = Nothing

            ' let's see if we need to process this "NEW" item as active or in-active item
            If oItem.IsActive Then
                '
                ' ACTIVE
                '

                ' create transaction objects
                oraTran = oraCN.BeginTransaction
                sql2012Tran = sql2012CN.BeginTransaction

                Try
                    '
                    ' add into ClassAvailableProducts
                    '
                    cmdSql = sql2012CN.CreateCommand
                    cmdSql.CommandText = "_sdiCreateClassAvailableProducts"
                    cmdSql.CommandType = CommandType.StoredProcedure
                    cmdSql.Transaction = sql2012Tran

                    parmSql = cmdSql.CreateParameter
                    parmSql.ParameterName = "@catalogId"
                    parmSql.SqlDbType = SqlDbType.BigInt
                    parmSql.Size = 0
                    parmSql.Value = oItem.CatalogId
                    parmSql.Direction = ParameterDirection.Input
                    cmdSql.Parameters.Add(parmSql)
                    parmSql = Nothing

                    parmSql = cmdSql.CreateParameter
                    parmSql.ParameterName = "@productViewId"
                    parmSql.SqlDbType = SqlDbType.BigInt
                    parmSql.Size = 0
                    parmSql.Value = oItem.Catalog_ProductViewId
                    parmSql.Direction = ParameterDirection.Input
                    cmdSql.Parameters.Add(parmSql)
                    parmSql = Nothing

                    parmSql = cmdSql.CreateParameter
                    parmSql.ParameterName = "@classId"
                    parmSql.SqlDbType = SqlDbType.BigInt
                    parmSql.Size = 0
                    parmSql.Value = oItem.Catalog_ClassId
                    parmSql.Direction = ParameterDirection.Input
                    cmdSql.Parameters.Add(parmSql)
                    parmSql = Nothing

                    parmSql = cmdSql.CreateParameter
                    parmSql.ParameterName = "@itemId"
                    parmSql.SqlDbType = SqlDbType.BigInt
                    parmSql.Size = 0
                    parmSql.Value = oItem.Catalog_ItemId
                    parmSql.Direction = ParameterDirection.Input
                    cmdSql.Parameters.Add(parmSql)
                    parmSql = Nothing

                    parmSql = cmdSql.CreateParameter
                    parmSql.ParameterName = "@customerItemId"
                    parmSql.SqlDbType = SqlDbType.NVarChar
                    parmSql.Size = 100
                    parmSql.Value = oItem.Catalog_CustomerItemId
                    parmSql.Direction = ParameterDirection.Input
                    cmdSql.Parameters.Add(parmSql)
                    parmSql = Nothing

                    i = cmdSql.ExecuteNonQuery

                    Try
                        cmdSql.Dispose()
                    Catch ex As Exception
                    End Try
                    cmdSql = Nothing

                    '
                    ' corect (add/update) into CP_JUNCTION
                    '   try updating first and if we got no record updated, insert new record
                    '
                    Dim oraCmd As OleDbCommand = Nothing
                    '   (1) update
                    sr = New System.IO.StreamReader(CStr(My.Settings("qryUpdateCP_JUNCTION")).Trim)
                    sql = sr.ReadToEnd
                    sr.Dispose()
                    sr = Nothing

                    ' fields 
                    sb = New System.Text.StringBuilder
                    sb.AppendFormat(sql, _
                                    oItem.SiteId, _
                                    oItem.PS_ItemId, _
                                    oItem.Catalog_ItemId, _
                                    "Y", _
                                    "Y")
                    sql = sb.ToString
                    sb = Nothing

                    oraCmd = oraCN.CreateCommand
                    oraCmd.CommandText = sql
                    oraCmd.CommandType = CommandType.Text
                    oraCmd.Transaction = oraTran

                    i = oraCmd.ExecuteNonQuery

                    Try
                        oraCmd.Dispose()
                    Catch ex As Exception
                    End Try
                    oraCmd = Nothing
                    '   (2) insert (if none were updated)
                    If (i = 0) Then
                        sr = New System.IO.StreamReader(CStr(My.Settings("qryInsertCP_JUNCTION")).Trim)
                        sql = sr.ReadToEnd
                        sr.Dispose()
                        sr = Nothing

                        ' fields 
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(sql, _
                                        oItem.SiteId, _
                                        oItem.PS_ItemId, _
                                        oItem.Catalog_ItemId)
                        sql = sb.ToString
                        sb = Nothing

                        oraCmd = oraCN.CreateCommand
                        oraCmd.CommandText = sql
                        oraCmd.CommandType = CommandType.Text
                        oraCmd.Transaction = oraTran

                        i = oraCmd.ExecuteNonQuery

                        Try
                            oraCmd.Dispose()
                        Catch ex As Exception
                        End Try
                        oraCmd = Nothing
                    End If

                    ' commit transactions
                    sql2012Tran.Commit()
                    oraTran.Commit()
                    logger.WriteVerboseLog(vbTab & "transaction committed")
                Catch ex As Exception
                    ' rollback transactions
                    sql2012Tran.Rollback()
                    oraTran.Rollback()
                    logger.WriteVerboseLog(vbTab & "transaction ROLLED BACK")
                End Try
            Else
                '
                ' INACTIVE
                '
                '       since this is a new BUT inactive item, don't even bother adding/correcting in both ClassAvailableProducts nor CP_JUNCTION tables
                '       we just need it in ScottsdaleTable so it won't be regarded as a new item next time we run this utility
            End If
        Else
            ' do we need to "activate" or "in-activate" this item
            If oItem.IsActive Then
                '
                ' make sure that item is ACTIVE
                '
                logger.WriteVerboseLog(vbTab & "processing as ACTIVE item")

                Dim nInActiveFlag As Integer = 0
                Dim cmdSql As SqlCommand = Nothing
                Dim i As Integer = -1

                ' since this is an existing item, grab the class ID for catalog/product view/item and update item object
                sr = New System.IO.StreamReader(CStr(My.Settings("qryGetItemClassId")).Trim)
                sql = sr.ReadToEnd
                sr.Dispose()
                sr = Nothing

                ' fields 
                sb = New System.Text.StringBuilder
                sb.AppendFormat(sql, _
                                oItem.CatalogId.ToString, _
                                oItem.CatalogId.ToString, _
                                oItem.Catalog_ProductViewId.ToString, _
                                oItem.Catalog_ItemId.ToString)
                sql = sb.ToString
                sb = Nothing

                cmdSql = sql2012CN.CreateCommand
                cmdSql.CommandText = sql
                cmdSql.CommandType = CommandType.Text

                Dim nClassId As Integer = 0

                Try
                    nClassId = CInt(cmdSql.ExecuteScalar)
                Catch ex As Exception
                End Try

                If (nClassId > 0) Then
                    oItem.Catalog_ClassId = nClassId
                End If

                Try
                    cmdSql.Dispose()
                Catch ex As Exception
                End Try
                cmdSql = Nothing

                If (oItem.Catalog_ClassId = 0) Then
                    sr = New System.IO.StreamReader(CStr(My.Settings("qryGetItemClassIdStaged")).Trim)
                    sql = sr.ReadToEnd
                    sr.Dispose()
                    sr = Nothing

                    ' fields 
                    sb = New System.Text.StringBuilder
                    sb.AppendFormat(sql, _
                                    oItem.CatalogId.ToString, _
                                    oItem.CatalogId.ToString, _
                                    oItem.Catalog_ProductViewId.ToString, _
                                    oItem.Catalog_ItemId.ToString)
                    sql = sb.ToString
                    sb = Nothing

                    cmdSql = sql2012CN.CreateCommand
                    cmdSql.CommandText = sql
                    cmdSql.CommandType = CommandType.Text

                    nClassId = 0

                    Try
                        nClassId = CInt(cmdSql.ExecuteScalar)
                    Catch ex As Exception
                    End Try

                    If (nClassId > 0) Then
                        oItem.Catalog_ClassId = nClassId
                    End If

                    Try
                        cmdSql.Dispose()
                    Catch ex As Exception
                    End Try
                    cmdSql = Nothing
                End If

                ' create transaction objects
                oraTran = oraCN.BeginTransaction
                sql2012Tran = sql2012CN.BeginTransaction

                Try
                    '   (1) copy back classAvailableProducts record from SDI_CPlus_Extend to origin catalog DB
                    sr = New System.IO.StreamReader(CStr(My.Settings("qryMainClassAvailProdUpdate")).Trim)
                    sql = sr.ReadToEnd
                    sr.Dispose()
                    sr = Nothing

                    Dim sCustId As String = oItem.Catalog_CustomerItemId
                    If (("~CUS~TBX").IndexOf(oItem.Catalog_CustomerItemPrefix) > -1) Then
                        sCustId = oItem.PS_ItemId
                    End If

                    ' fields 
                    sb = New System.Text.StringBuilder
                    sb.AppendFormat(sql, _
                                    oItem.CatalogId.ToString, _
                                    oItem.CatalogId.ToString, _
                                    oItem.Catalog_ProductViewId.ToString, _
                                    oItem.Catalog_ClassId.ToString, _
                                    oItem.Catalog_ItemId.ToString, _
                                    sCustId)
                    sql = sb.ToString
                    sb = Nothing

                    cmdSql = sql2012CN.CreateCommand
                    cmdSql.CommandText = sql
                    cmdSql.CommandType = CommandType.Text
                    cmdSql.Transaction = sql2012Tran

                    i = -1
                    i = cmdSql.ExecuteNonQuery

                    Try
                        cmdSql.Dispose()
                    Catch ex As Exception
                    End Try
                    cmdSql = Nothing

                    If (i < 1) Then
                        sr = New System.IO.StreamReader(CStr(My.Settings("qryMainClassAvailProdInsert")).Trim)
                        sql = sr.ReadToEnd
                        sr.Dispose()
                        sr = Nothing

                        ' fields 
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(sql, _
                                        oItem.CatalogId.ToString, _
                                        oItem.CatalogId.ToString, _
                                        oItem.Catalog_ProductViewId.ToString, _
                                        oItem.Catalog_ClassId.ToString, _
                                        oItem.Catalog_ItemId.ToString)
                        sql = sb.ToString
                        sb = Nothing

                        cmdSql = sql2012CN.CreateCommand
                        cmdSql.CommandText = sql
                        cmdSql.CommandType = CommandType.Text
                        cmdSql.Transaction = sql2012Tran

                        i = -1
                        i = cmdSql.ExecuteNonQuery

                        Try
                            cmdSql.Dispose()
                        Catch ex As Exception
                        End Try
                        cmdSql = Nothing
                    End If

                    '   (2) turn OFF "in-active" (if found) flag on classAvailableProducts in SDI_CPlus_Extend DB
                    sr = New System.IO.StreamReader(CStr(My.Settings("qryStageClassAvailProdUpdateFlag")).Trim)
                    sql = sr.ReadToEnd
                    sr.Dispose()
                    sr = Nothing

                    ' fields 
                    sb = New System.Text.StringBuilder
                    sb.AppendFormat(sql, _
                                    oItem.CatalogId.ToString, _
                                    oItem.Catalog_ProductViewId.ToString, _
                                    oItem.Catalog_ClassId.ToString, _
                                    oItem.Catalog_ItemId.ToString, _
                                    nInActiveFlag.ToString)
                    sql = sb.ToString
                    sb = Nothing

                    cmdSql = sql2012CN.CreateCommand
                    cmdSql.CommandText = sql
                    cmdSql.CommandType = CommandType.Text
                    cmdSql.Transaction = sql2012Tran

                    i = -1
                    i = cmdSql.ExecuteNonQuery

                    Try
                        cmdSql.Dispose()
                    Catch ex As Exception
                    End Try
                    cmdSql = Nothing

                    '   (3) correct (insert/update) CP_JUNCTION table
                    Dim oraCmd As OleDbCommand = Nothing

                    sr = New System.IO.StreamReader(CStr(My.Settings("qryUpdateCP_JUNCTION")).Trim)
                    sql = sr.ReadToEnd
                    sr.Dispose()
                    sr = Nothing

                    ' fields 
                    sb = New System.Text.StringBuilder
                    sb.AppendFormat(sql, _
                                    oItem.SiteId, _
                                    oItem.PS_ItemId, _
                                    oItem.Catalog_ItemId, _
                                    "Y", _
                                    "Y")
                    sql = sb.ToString
                    sb = Nothing

                    oraCmd = oraCN.CreateCommand
                    oraCmd.CommandText = sql
                    oraCmd.CommandType = CommandType.Text
                    oraCmd.Transaction = oraTran

                    i = -1
                    i = oraCmd.ExecuteNonQuery

                    Try
                        oraCmd.Dispose()
                    Catch ex As Exception
                    End Try
                    oraCmd = Nothing

                    If (i < 1) Then
                        sr = New System.IO.StreamReader(CStr(My.Settings("qryInsertCP_JUNCTION")).Trim)
                        sql = sr.ReadToEnd
                        sr.Dispose()
                        sr = Nothing

                        ' fields 
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(sql, _
                                        oItem.SiteId, _
                                        oItem.PS_ItemId, _
                                        oItem.Catalog_ItemId, _
                                        "Y", _
                                        "Y")
                        sql = sb.ToString
                        sb = Nothing

                        oraCmd = oraCN.CreateCommand
                        oraCmd.CommandText = sql
                        oraCmd.CommandType = CommandType.Text
                        oraCmd.Transaction = oraTran

                        i = -1
                        i = oraCmd.ExecuteNonQuery

                        Try
                            oraCmd.Dispose()
                        Catch ex As Exception
                        End Try
                        oraCmd = Nothing
                    End If

                    '   (4) catch-up code to update the mfg/mfg part number of a given item if already in staging table
                    If (oItem.Catalog_ItemId > 9100000) Then
                        sr = New System.IO.StreamReader(CStr(My.Settings("qryUpdateScottsdaleItemMfgPartNo")).Trim)
                        sql = sr.ReadToEnd
                        sr.Dispose()
                        sr = Nothing

                        Dim mfgId As String = oItem.PS_ManufacturerId.Trim
                        If (mfgId.Length = 0) Then
                            mfgId = "NULL"
                        Else
                            mfgId = "'" & mfgId & "'"
                        End If

                        Dim mfgPart As String = oItem.PS_ManufacturerPartNumber.Trim
                        If (mfgPart.Length = 0) Then
                            mfgPart = "NULL"
                        Else
                            'mfgPart = "'" & mfgPart.Replace("'", "''") & "'"
                            mfgPart = mfgPart.Replace("'", "")
                            mfgPart = mfgPart.Replace("""", "")
                            mfgPart = "'" & mfgPart & "'"
                        End If

                        ' fields 
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(sql, _
                                        oItem.Catalog_ClassId, _
                                        oItem.Catalog_ItemId, _
                                        mfgId, _
                                        mfgPart)
                        sql = sb.ToString
                        sb = Nothing

                        cmdSql = sql2012CN.CreateCommand
                        cmdSql.CommandText = sql
                        cmdSql.CommandType = CommandType.Text
                        cmdSql.Transaction = sql2012Tran

                        i = -1
                        i = cmdSql.ExecuteNonQuery

                        Try
                            cmdSql.Dispose()
                        Catch ex As Exception
                        End Try
                        cmdSql = Nothing
                    End If

                    ' commit transactions
                    sql2012Tran.Commit()
                    oraTran.Commit()
                    logger.WriteVerboseLog(vbTab & "transaction committed")
                Catch ex As Exception
                    ' rollback transactions
                    sql2012Tran.Rollback()
                    oraTran.Rollback()
                    logger.WriteVerboseLog(vbTab & "transaction ROLLED BACK")
                End Try
            Else
                '
                ' IN-ACTIVATE item
                '
                logger.WriteVerboseLog(vbTab & "processing as IN-ACTIVE item")

                Dim nInActiveFlag As Integer = 1
                Dim cmdSql As SqlCommand = Nothing
                Dim i As Integer = -1

                ' create transaction objects
                oraTran = oraCN.BeginTransaction
                sql2012Tran = sql2012CN.BeginTransaction

                Try
                    '   (1) copy from origin catalog DB into SDI_CPlus_Extend, the classAvailableProduct record with "in-active" flag
                    sr = New System.IO.StreamReader(CStr(My.Settings("qryStageClassAvailProdUpdate")).Trim)
                    sql = sr.ReadToEnd
                    sr.Dispose()
                    sr = Nothing

                    ' fields 
                    sb = New System.Text.StringBuilder
                    sb.AppendFormat(sql, _
                                    oItem.CatalogId.ToString, _
                                    oItem.CatalogId.ToString, _
                                    oItem.Catalog_ProductViewId.ToString, _
                                    oItem.Catalog_ClassId.ToString, _
                                    oItem.Catalog_ItemId.ToString, _
                                    nInActiveFlag.ToString)
                    sql = sb.ToString
                    sb = Nothing

                    cmdSql = sql2012CN.CreateCommand
                    cmdSql.CommandText = sql
                    cmdSql.CommandType = CommandType.Text
                    cmdSql.Transaction = sql2012Tran

                    i = -1
                    i = cmdSql.ExecuteNonQuery

                    Try
                        cmdSql.Dispose()
                    Catch ex As Exception
                    End Try
                    cmdSql = Nothing

                    If (i < 1) Then
                        sr = New System.IO.StreamReader(CStr(My.Settings("qryStageClassAvailProdInsert")).Trim)
                        sql = sr.ReadToEnd
                        sr.Dispose()
                        sr = Nothing

                        ' fields 
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(sql, _
                                        oItem.CatalogId.ToString, _
                                        oItem.CatalogId.ToString, _
                                        oItem.Catalog_ProductViewId.ToString, _
                                        oItem.Catalog_ClassId.ToString, _
                                        oItem.Catalog_ItemId.ToString, _
                                        nInActiveFlag.ToString)
                        sql = sb.ToString
                        sb = Nothing

                        cmdSql = sql2012CN.CreateCommand
                        cmdSql.CommandText = sql
                        cmdSql.CommandType = CommandType.Text
                        cmdSql.Transaction = sql2012Tran

                        i = -1
                        i = cmdSql.ExecuteNonQuery

                        Try
                            cmdSql.Dispose()
                        Catch ex As Exception
                        End Try
                        cmdSql = Nothing
                    End If

                    '   (2) remove from origin catalog DB, the classAvailableProduct record
                    sr = New System.IO.StreamReader(CStr(My.Settings("qryRemoveOriginClassAvailProd")).Trim)
                    sql = sr.ReadToEnd
                    sr.Dispose()
                    sr = Nothing

                    ' fields 
                    sb = New System.Text.StringBuilder
                    sb.AppendFormat(sql, _
                                    oItem.CatalogId.ToString, _
                                    oItem.CatalogId.ToString, _
                                    oItem.Catalog_ProductViewId.ToString, _
                                    oItem.Catalog_ClassId.ToString, _
                                    oItem.Catalog_ItemId.ToString)
                    sql = sb.ToString
                    sb = Nothing

                    cmdSql = sql2012CN.CreateCommand
                    cmdSql.CommandText = sql
                    cmdSql.CommandType = CommandType.Text
                    cmdSql.Transaction = sql2012Tran

                    i = -1
                    i = cmdSql.ExecuteNonQuery

                    Try
                        cmdSql.Dispose()
                    Catch ex As Exception
                    End Try
                    cmdSql = Nothing

                    '   (3) take off commodity code from cp_junction table for this item
                    Dim oraCmd As OleDbCommand = Nothing

                    sr = New System.IO.StreamReader(CStr(My.Settings("qryUpdateCP_JUNCTION")).Trim)
                    sql = sr.ReadToEnd
                    sr.Dispose()
                    sr = Nothing

                    ' fields 
                    sb = New System.Text.StringBuilder
                    sb.AppendFormat(sql, _
                                    oItem.SiteId, _
                                    oItem.PS_ItemId, _
                                    " ", _
                                    " ", _
                                    " ")
                    sql = sb.ToString
                    sb = Nothing

                    oraCmd = oraCN.CreateCommand
                    oraCmd.CommandText = sql
                    oraCmd.CommandType = CommandType.Text
                    oraCmd.Transaction = oraTran

                    i = -1
                    i = oraCmd.ExecuteNonQuery

                    Try
                        oraCmd.Dispose()
                    Catch ex As Exception
                    End Try
                    oraCmd = Nothing

                    ' commit transactions
                    sql2012Tran.Commit()
                    oraTran.Commit()
                    logger.WriteVerboseLog(vbTab & "transaction committed")
                Catch ex As Exception
                    ' rollback transactions
                    sql2012Tran.Rollback()
                    oraTran.Rollback()
                    logger.WriteVerboseLog(vbTab & "transaction ROLLED BACK")
                End Try
            End If
        End If

        Try
            sql2012Tran.Dispose()
        Catch ex As Exception
        End Try
        sql2012Tran = Nothing

        Try
            oraTran.Dispose()
        Catch ex As Exception
        End Try
        oraTran = Nothing

        sb = Nothing
        sr = Nothing
    End Sub

    Private Function IsNewItem(ByVal oItem As item, _
                               ByVal sql2012CN As SqlConnection) As Boolean
        Dim sRtn As String = "IsNewItem"
        Dim bExit As Boolean = False
        ' let's assume (default) that we're starting with a new item
        Dim bIsNewItem As Boolean = True

        Dim sql As String = ""
        Dim sb As System.Text.StringBuilder = Nothing
        Dim sr As System.IO.StreamReader = Nothing
        Dim sqlCmd As SqlCommand = Nothing
        Dim sqlRdr As SqlDataReader = Nothing

        ' (1) if this item already have a "commodity code" assigned to it,
        '       then this item already exists
        If bIsNewItem And Not bExit Then
            If (oItem.Catalog_ItemId > 0) Then
                ' I'm not a new item
                bIsNewItem = False
                ' exit out of this routine
                bExit = True
            End If
        End If

        ' (2) check against SQL2012 ScottsdaleItemTable/ClassAvailableProducts for the "customer item ID"
        '       if existing if item did not seem to carry a commodity code
        If bIsNewItem And Not bExit Then
            ' check for any possibility that the customer item Id already existing
            sr = New System.IO.StreamReader(CStr(My.Settings("qryCheckCustomerItemInCatalog")).Trim)
            sql = sr.ReadToEnd
            sr.Dispose()
            sr = Nothing

            sb = New System.Text.StringBuilder
            sb.AppendFormat(sql, _
                            oItem.PS_ItemId, _
                            oItem.CatalogId.ToString, _
                            oItem.Catalog_ProductViewId.ToString, _
                            oItem.Catalog_CustomerItemId)
            sql = sb.ToString
            sb = Nothing

            sqlCmd = sql2012CN.CreateCommand
            sqlCmd.CommandText = sql
            sqlCmd.CommandType = CommandType.Text

            sqlRdr = Nothing

            Try
                sqlRdr = sqlCmd.ExecuteReader
            Catch ex As Exception
            End Try

            If Not (sqlRdr Is Nothing) Then
                Dim nCatId As Integer = 0
                Dim nClassId As Integer = 0
                Dim nItemId As Integer = 0
                While sqlRdr.Read
                    Try
                        nCatId = CInt(sqlRdr("catalogID"))
                    Catch ex As Exception
                    End Try
                    Try
                        nClassId = CInt(sqlRdr("classID"))
                    Catch ex As Exception
                    End Try
                    Try
                        nItemId = CInt(sqlRdr("itemID"))
                    Catch ex As Exception
                    End Try
                End While
                oItem.Catalog_ClassId = nClassId
                oItem.Catalog_ItemId = nItemId
                If (oItem.Catalog_ItemId > 0) Then
                    bIsNewItem = False
                    bExit = True
                End If
            End If

            Try
                sqlRdr.Close()
            Catch ex As Exception
            End Try
            sqlRdr = Nothing

            Try
                sqlCmd.Dispose()
            Catch ex As Exception
            End Try
            sqlCmd = Nothing
        End If

        Return (bIsNewItem)
    End Function

End Module
