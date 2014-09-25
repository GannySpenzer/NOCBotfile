Imports System.Data.OleDb
Imports System.Data.SqlClient


Module CPJunctionSync

    Sub Main()

        Const PARM_SITE_ID As String = "/SITE"

        Dim logger As SDI.ApplicationLogger.appLogger = Nothing
        Dim logPath As String = ""
        Dim logFile As String = ""
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        ' this app name/version
        Dim meName As String = System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName
        Dim meVer As String = System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString

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

        While (logPath.Length > 0) And (logPath.Trim.LastIndexOf("\"c) = (logPath.Trim.Length - 1))
            logPath = logPath.Trim.TrimEnd("\"c)
        End While
        If Not System.IO.Directory.Exists(logPath) Then
            System.IO.Directory.CreateDirectory(logPath)
        End If

        ' log filename ID
        Dim sLogFilenameId As String = "SDI.CPJunctionSync"
        's = ""
        'Try
        '    s = CStr(My.Settings(propertyName:="appLogFilenameId")).Trim
        'Catch ex As Exception
        '    s = ""
        'End Try
        'If (s.Length > 0) Then
        '    sLogFilenameId = s
        'End If

        ' site to process
        Dim sSiteId As String = ""
        Dim cmdArgs As Hashtable = CommandLineArgs()
        If (cmdArgs.ContainsKey(PARM_SITE_ID)) Then
            sSiteId = CStr(cmdArgs(PARM_SITE_ID))
        End If

        ' log file
        logFile = logPath & "\" & sLogFilenameId & "." & sSiteId & "_" & Guid.NewGuid.ToString & ".log"

        ' logger
        logger = New SDI.ApplicationLogger.appLogger(logFile, logLevel)
        logger.WriteInformationLog("starting " & meName & " v" & meVer)
        logger.WriteInformationLog("site ID : " & sSiteId)

        Dim sql As String = ""
        Dim sb As System.Text.StringBuilder = Nothing
        Dim sr As System.IO.StreamReader = Nothing

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

        Dim sql2012CN As SqlConnection = Nothing

        '
        ' build site/product view cross reference
        '
        sql2012CN = New SqlConnection(CStr(My.Settings("sql2012CNString")).Replace("{0}", "SDI_CPlus_Extend"))
        sql2012CN.Open()
        sr = New System.IO.StreamReader(CStr(My.Settings("qryGetSiteProdViewList")).Trim)
        sql = sr.ReadToEnd
        sr.Dispose()
        sr = Nothing
        Dim oSiteProductViews As SiteProductViews = New SiteProductViews(sql2012CN, sql, oCatalogProductViews)
        Try
            sql2012CN.Close()
        Catch ex As Exception
        End Try
        Try
            sql2012CN.Dispose()
        Catch ex As Exception
        End Try
        sql2012CN = Nothing
        logger.WriteVerboseLog("site/product views (count) : " & oSiteProductViews.Item.Count.ToString)

        ' check if we can process site
        If sSiteId.Trim.Length > 0 Then
            Dim arrSite As ArrayList = oSiteProductViews.GetSite(siteId:=sSiteId)
            If arrSite.Count > 0 Then
                For Each oSite As SiteProductView In arrSite
                    ProcessSite(oSite, logger)
                Next
            Else
                logger.WriteWarningLog("UNABLE TO IDENTIFY SITE : " & sSiteId)
            End If
        Else
            logger.WriteWarningLog("SITE TO PROCESS NOT SPECIFIED")
        End If

        ' finished processing
        logger.WriteInformationLog("finished executing " & meName)

        ' clean-up
        Try
            sql2012CN.Close()
        Catch ex As Exception
        End Try
        Try
            sql2012CN.Dispose()
        Catch ex As Exception
        End Try
        sql2012CN = Nothing

        logLevel = Nothing
        logger = Nothing

    End Sub

    Private Function CommandLineArgs() As Hashtable
        Dim arr As New Hashtable
        For Each sArg As String In My.Application.CommandLineArgs
            If (sArg.Trim.Length > 0) Then
                Dim sKey As String = ""
                Dim sValue As String = ""
                Dim s1 As String() = New String() {}
                Try
                    s1 = sArg.Split("="c)
                Catch ex As Exception
                End Try
                If (s1.Length > 1) Then
                    sKey = s1(0).Trim.ToUpper
                    sValue = s1(1).Trim
                End If
                If (sKey.Length > 0) And (sValue.Length > 0) Then
                    If arr.ContainsKey(sKey) Then
                        arr(sKey) = sValue
                    Else
                        arr.Add(sKey, sValue)
                    End If
                End If
            End If
        Next
        Return (arr)
    End Function

    Private Sub ProcessSite(ByVal oSite As SiteProductView, _
                            ByVal logger As SDI.ApplicationLogger.IApplicationLogger)

        Dim sql As String = ""
        Dim sb As System.Text.StringBuilder = Nothing
        Dim sr As System.IO.StreamReader = Nothing

        Dim oraCN As OleDbConnection = Nothing
        oraCN = New OleDbConnection(connectionString:=CStr(My.Settings("oraCNString")))

        Dim sql2012CN As SqlConnection = Nothing
        sql2012CN = New SqlConnection(CStr(My.Settings("sql2012CNString")).Replace("{0}", "SDI_CPlus_Extend"))

        '
        ' CP_JUNCTION view
        '       iterate through cp_junction for this site
        '

        oraCN.Open()

        If (oraCN.State = ConnectionState.Open) Then

            sr = New System.IO.StreamReader(CStr(My.Settings("qryGetCPJunctionItemListForSite")).Trim)
            sql = sr.ReadToEnd
            sr.Dispose()
            sr = Nothing

            ' fields 
            sb = New System.Text.StringBuilder
            sb.AppendFormat(sql, _
                            oSite.SiteId)
            sql = sb.ToString
            sb = Nothing

            Dim cmdCPJunctionItems As OleDbCommand = oraCN.CreateCommand
            cmdCPJunctionItems.CommandText = sql
            cmdCPJunctionItems.CommandType = CommandType.Text

            Dim rdrCPJunctionItems As OleDbDataReader = Nothing

            Try
                rdrCPJunctionItems = cmdCPJunctionItems.ExecuteReader
            Catch ex As Exception
            End Try

            If Not (rdrCPJunctionItems Is Nothing) Then

                Dim siteId As String = ""
                Dim invItemId As String = ""
                Dim customerItemId As String = ""
                Dim itemId As Integer = 0

                ' for checking classAvailableProducts for an item
                sr = New System.IO.StreamReader(CStr(My.Settings("qryGetClassAvailProductItemCount")).Trim)
                Dim sqlCheckClassAvail As String = sr.ReadToEnd
                sr.Dispose()
                sr = Nothing

                sr = New System.IO.StreamReader(CStr(My.Settings("qryGetClassAvailProductItemCount_CustItemId")).Trim)
                Dim sqlCheckClassAvailCustItemId As String = sr.ReadToEnd
                sr.Dispose()
                sr = Nothing

                '' for updating cp_junction via part#
                'sr = New System.IO.StreamReader(CStr(My.Settings("qryCPJunctionUpdate")).Trim)
                'Dim sqlUpdateCPJunction As String = sr.ReadToEnd
                'sr.Dispose()
                'sr = Nothing

                ' for updating cp_junction (if commodity code is not the same with existing)
                sr = New System.IO.StreamReader(CStr(My.Settings("qryCPJunctionUpdateViaPartNoNotEqual")).Trim)
                Dim sqlUpdateCPJunctionViaPartNoIfNotEqual As String = sr.ReadToEnd
                sr.Dispose()
                sr = Nothing

                sql2012CN.Open()

                Dim oraModCN As OleDbConnection = New OleDbConnection(connectionString:=CStr(My.Settings("oraCNString")))
                oraModCN.Open()

                While rdrCPJunctionItems.Read
                    siteId = ""
                    Try
                        siteId = CStr(rdrCPJunctionItems("ISA_SITE_ID")).Trim.ToUpper
                    Catch ex As Exception
                    End Try
                    invItemId = ""
                    Try
                        invItemId = CStr(rdrCPJunctionItems("INV_ITEM_ID")).Trim.ToUpper
                    Catch ex As Exception
                    End Try
                    ' as of the writing of this code, there's only 2 site indicator flag at the moment
                    '       "A" which means it's active and customerItemID value is INV_ITEM_ID WITHOUT the prefix
                    '       "S" which means it's special and customerItemID value IS EQUAL to INV_ITEM_ID (INCLUDING the prefix); one-to-one
                    customerItemId = ""
                    If (oSite.SiteIndicatorFlag = "S") Then
                        customerItemId = invItemId
                    Else
                        customerItemId = invItemId.Substring(oSite.SiteItemPrefix.Length, (invItemId.Length - oSite.SiteItemPrefix.Length))
                    End If
                    itemId = 0
                    Try
                        itemId = CInt(rdrCPJunctionItems("ISA_CP_PROD_ID"))
                    Catch ex As Exception
                    End Try
                    If (itemId > 0) And (invItemId.Substring(0, 3) = oSite.SiteItemPrefix) Then
                        ' look in classAvailableProducts (catalog / product view / item ID)
                        '   IF NOT FOUND, blank ISA_CP_PROD_ID of CP_JUNCTION :: this means that this item (itemID) was disabled in ContentPlus for this site/product view
                        sql = sqlCheckClassAvail
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(sql, _
                                        oSite.CatalogId.ToString, _
                                        oSite.CatalogId.ToString, _
                                        oSite.ProductViewId.ToString, _
                                        itemId)
                        sql = sb.ToString
                        sb = Nothing
                        Dim cmdSql As SqlCommand = sql2012CN.CreateCommand
                        cmdSql.CommandText = sql
                        cmdSql.CommandType = CommandType.Text
                        Dim nCount As Integer = 0
                        Try
                            nCount = CInt(cmdSql.ExecuteScalar)
                        Catch ex As Exception
                        End Try
                        Try
                            cmdSql.Dispose()
                        Catch ex As Exception
                        End Try
                        cmdSql = Nothing
                        If (nCount < 1) Then
                            ' item NOT FOUND in ClassAvailableProducts, so let's blank out CP_JUNCTION (if not blanked out yet)
                            sql = sqlUpdateCPJunctionViaPartNoIfNotEqual
                            sb = New System.Text.StringBuilder
                            sb.AppendFormat(sql, _
                                            oSite.SiteId, _
                                            invItemId, _
                                            " ", _
                                            "Y", _
                                            "Y")
                            sql = sb.ToString
                            sb = Nothing
                            Dim cmdOraMod As OleDbCommand = oraModCN.CreateCommand
                            cmdOraMod.CommandText = sql
                            cmdOraMod.CommandType = CommandType.Text
                            nCount = cmdOraMod.ExecuteNonQuery
                            If (nCount > 0) Then
                                logger.WriteVerboseLog("item " & oSite.CatalogId.ToString & " / " & oSite.ProductViewId.ToString & " / " & itemId.ToString & " (" & invItemId & "/" & customerItemId & ") was deactivated in CP_JUNCTION :: cannot find item ID in ClassAvailableProducts table")
                            End If
                            Try
                                cmdOraMod.Dispose()
                            Catch ex As Exception
                            End Try
                            cmdOraMod = Nothing
                        End If
                    End If
                    If (customerItemId.Length > 0) And (invItemId.Substring(0, 3) = oSite.SiteItemPrefix) Then
                        ' look in classAvailableProducts (catalog / product view / customerItemID)
                        '   IF NOT FOUND, blank ISA_CP_PROD_ID of CP_JUNCTION :: this means that this item (customerItemID) was disabled in ContentPlus for this site/product view
                        '   IF FOUND, might be more than one (same customerItemID with different commodity code), ignore ... let the process from classAvailableProducts to CP_JUNCTION handle updating the commodity code
                        sql = sqlCheckClassAvailCustItemId
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(sql, _
                                        oSite.CatalogId.ToString, _
                                        oSite.CatalogId.ToString, _
                                        oSite.ProductViewId.ToString, _
                                        customerItemId)
                        sql = sb.ToString
                        sb = Nothing
                        Dim cmdSql As SqlCommand = sql2012CN.CreateCommand
                        cmdSql.CommandText = sql
                        cmdSql.CommandType = CommandType.Text
                        Dim nCount As Integer = 0
                        Try
                            nCount = CInt(cmdSql.ExecuteScalar)
                        Catch ex As Exception
                        End Try
                        Try
                            cmdSql.Dispose()
                        Catch ex As Exception
                        End Try
                        cmdSql = Nothing
                        If (nCount < 1) Then
                            ' item NOT FOUND in ClassAvailableProducts, so let's blank out CP_JUNCTION
                            sql = sqlUpdateCPJunctionViaPartNoIfNotEqual
                            sb = New System.Text.StringBuilder
                            sb.AppendFormat(sql, _
                                            oSite.SiteId, _
                                            invItemId, _
                                            " ", _
                                            "Y", _
                                            "Y")
                            sql = sb.ToString
                            sb = Nothing
                            Dim cmdOraMod As OleDbCommand = oraModCN.CreateCommand
                            cmdOraMod.CommandText = sql
                            cmdOraMod.CommandType = CommandType.Text
                            nCount = cmdOraMod.ExecuteNonQuery
                            If (nCount > 0) Then
                                logger.WriteVerboseLog("item " & oSite.CatalogId.ToString & " / " & oSite.ProductViewId.ToString & " / " & itemId.ToString & " (" & invItemId & "/" & customerItemId & ") was deactivated in CP_JUNCTION :: cannot find customer part# in ClassAvailableProducts table")
                            End If
                            Try
                                cmdOraMod.Dispose()
                            Catch ex As Exception
                            End Try
                            cmdOraMod = Nothing
                        End If
                    End If
                End While

                Try
                    oraModCN.Close()
                Catch ex As Exception
                End Try
                Try
                    oraModCN.Dispose()
                Catch ex As Exception
                End Try
                oraModCN = Nothing

                Try
                    sql2012CN.Close()
                Catch ex As Exception
                End Try

            End If

            Try
                rdrCPJunctionItems.Close()
            Catch ex As Exception
            End Try
            rdrCPJunctionItems = Nothing

            Try
                cmdCPJunctionItems.Dispose()
            Catch ex As Exception
            End Try
            cmdCPJunctionItems = Nothing

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

        sb = Nothing
        sr = Nothing

        '
        ' ClassAvailableProducts view
        '       iterate through classAvailableProducts for this catalog/product view
        '

        sql2012CN.Open()

        If (sql2012CN.State = ConnectionState.Open) Then

            sr = New System.IO.StreamReader(CStr(My.Settings("qryGetClassAvailableProductItemsForProdView")).Trim)
            sql = sr.ReadToEnd
            sr.Dispose()
            sr = Nothing

            ' fields 
            sb = New System.Text.StringBuilder
            sb.AppendFormat(sql, _
                            oSite.CatalogId, _
                            oSite.CatalogId, _
                            oSite.ProductViewId)
            sql = sb.ToString
            sb = Nothing

            Dim cmdClassAvailItems As SqlCommand = sql2012CN.CreateCommand
            cmdClassAvailItems.CommandText = sql
            cmdClassAvailItems.CommandType = CommandType.Text

            Dim rdrClassAvailItems As SqlDataReader = Nothing

            Try
                rdrClassAvailItems = cmdClassAvailItems.ExecuteReader
            Catch ex As Exception
            End Try

            If Not (rdrClassAvailItems Is Nothing) Then

                Dim catalogId As Integer = 0
                Dim productViewId As Integer = 0
                Dim classId As Integer = 0
                Dim itemId As Integer = 0
                Dim customerItemId As String = ""
                Dim invItemId As String = ""

                ' for checking cp_junction via part#
                sr = New System.IO.StreamReader(CStr(My.Settings("qryCheckCPJunctionViaPartNumber")).Trim)
                Dim sqlCheckCPJunctionViaPartNo As String = sr.ReadToEnd
                sr.Dispose()
                sr = Nothing

                '' for updating cp_junction via part#
                'sr = New System.IO.StreamReader(CStr(My.Settings("qryCPJunctionUpdate")).Trim)
                'Dim sqlUpdateCPJunction As String = sr.ReadToEnd
                'sr.Dispose()
                'sr = Nothing

                '' for updating cp_junction via commodity code
                'sr = New System.IO.StreamReader(CStr(My.Settings("qryCPJunctionUpdateViaItemID")).Trim)
                'Dim sqlUpdateCPJunctionViaItemId As String = sr.ReadToEnd
                'sr.Dispose()
                'sr = Nothing

                ' for updating cp_junction via commodity code (if commodity code is not the same with existing)
                sr = New System.IO.StreamReader(CStr(My.Settings("qryCPJunctionUpdateViaItemID")).Trim)
                Dim sqlUpdateCPJunctionViaItemIdIfNotEqual As String = sr.ReadToEnd
                sr.Dispose()
                sr = Nothing

                ' for adding a new record in cp_junction
                sr = New System.IO.StreamReader(CStr(My.Settings("qryCPJunctionAdd")).Trim)
                Dim sqlCreateCPJunction As String = sr.ReadToEnd
                sr.Dispose()
                sr = Nothing

                ' for updating cp_junction (if commodity code is not the same with existing)
                sr = New System.IO.StreamReader(CStr(My.Settings("qryCPJunctionUpdateViaPartNoNotEqual")).Trim)
                Dim sqlUpdateCPJunctionViaPartNoIfNotEqual As String = sr.ReadToEnd
                sr.Dispose()
                sr = Nothing

                Dim oraModCN As OleDbConnection = New OleDbConnection(connectionString:=CStr(My.Settings("oraCNString")))
                oraModCN.Open()

                While rdrClassAvailItems.Read
                    catalogId = 0
                    Try
                        catalogId = CInt(rdrClassAvailItems("catalogID"))
                    Catch ex As Exception
                    End Try
                    productViewId = 0
                    Try
                        productViewId = CInt(rdrClassAvailItems("productViewID"))
                    Catch ex As Exception
                    End Try
                    classId = 0
                    Try
                        classId = CInt(rdrClassAvailItems("classID"))
                    Catch ex As Exception
                    End Try
                    itemId = 0
                    Try
                        itemId = CInt(rdrClassAvailItems("itemID"))
                    Catch ex As Exception
                    End Try
                    customerItemId = ""
                    Try
                        customerItemId = CStr(rdrClassAvailItems("customerItemID")).Trim
                    Catch ex As Exception
                    End Try
                    ' as of the writing of this code, there's only 2 site indicator flag at the moment
                    '       "A" which means it's active and customerItemID value is INV_ITEM_ID WITHOUT the prefix
                    '       "S" which means it's special and customerItemID value IS EQUAL to INV_ITEM_ID (INCLUDING the prefix); one-to-one
                    invItemId = ""
                    If (oSite.SiteIndicatorFlag = "S") Then
                        invItemId = customerItemId
                    Else
                        invItemId = (oSite.SiteItemPrefix & customerItemId)
                    End If

                    If (customerItemId.Length = 0) Or (customerItemId = "0") Or ((customerItemId & "~") = (oSite.SiteItemPrefix & "~")) Then
                        ' current item has a blank/empty customer item ID
                        '   so let's search and disable in CP_JUNCTION for site (site ID + item ID)
                        sql = sqlUpdateCPJunctionViaItemIdIfNotEqual
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(sql, _
                                        oSite.SiteId, _
                                        itemId.ToString, _
                                        " ", _
                                        "Y", _
                                        "Y")
                        sql = sb.ToString
                        sb = Nothing
                        Dim cmdOraMod As OleDbCommand = oraModCN.CreateCommand
                        cmdOraMod.CommandText = sql
                        cmdOraMod.CommandType = CommandType.Text
                        Dim nCount As Integer = cmdOraMod.ExecuteNonQuery
                        If (nCount > 0) Then
                            logger.WriteVerboseLog("item " & oSite.CatalogId.ToString & " / " & oSite.ProductViewId.ToString & " / " & itemId.ToString & " (" & invItemId & "/" & customerItemId & ") was de-activated in CP_JUNCTION : blank/0 customer part# in ClassAvailableProducts table")
                        End If
                        Try
                            cmdOraMod.Dispose()
                        Catch ex As Exception
                        End Try
                        cmdOraMod = Nothing
                    End If

                    If (customerItemId.Length > 0) And (customerItemId <> "0") And ((customerItemId & "~") <> (oSite.SiteItemPrefix & "~")) Then
                        ' search in cp_junction for site id + inv_item_id
                        '   IF NOT FOUND, add in CP_JUNCTION
                        '   IF FOUND BUT DIFFERENT item ID, update item ID in CP_JUNCTION
                        '   IF FOUND AND SAME item ID, DO NOTHING
                        sql = sqlCheckCPJunctionViaPartNo
                        sb = New System.Text.StringBuilder
                        sb.AppendFormat(sql, _
                                        oSite.SiteId, _
                                        invItemId)
                        sql = sb.ToString
                        sb = Nothing
                        Dim cmdOraMod As OleDbCommand = oraModCN.CreateCommand
                        cmdOraMod.CommandText = sql
                        cmdOraMod.CommandType = CommandType.Text
                        Dim nISA_CP_PROD_ID As Integer = -1
                        Try
                            nISA_CP_PROD_ID = CInt(cmdOraMod.ExecuteScalar)
                        Catch ex As Exception
                        End Try
                        Try
                            cmdOraMod.Dispose()
                        Catch ex As Exception
                        End Try
                        cmdOraMod = Nothing
                        If (nISA_CP_PROD_ID < 1) Then
                            ' site id + inv_item_id NOT FOUND, ADD into CP_JUNCTION
                            sql = sqlCreateCPJunction
                            sb = New System.Text.StringBuilder
                            sb.AppendFormat(sql, _
                                            oSite.SiteId, _
                                            invItemId, _
                                            " ", _
                                            itemId.ToString, _
                                            "Y", _
                                            "Y")
                            sql = sb.ToString
                            sb = Nothing
                            cmdOraMod = oraModCN.CreateCommand
                            cmdOraMod.CommandText = sql
                            cmdOraMod.CommandType = CommandType.Text
                            Dim nCount As Integer = cmdOraMod.ExecuteNonQuery
                            If (nCount > 0) Then
                                logger.WriteVerboseLog("site/item " & oSite.SiteId & " / " & invItemId & " / " & itemId.ToString & " NOT FOUND in CP_JUNCTION : ADDING")
                            End If
                            Try
                                cmdOraMod.Dispose()
                            Catch ex As Exception
                            End Try
                            cmdOraMod = Nothing
                        ElseIf (nISA_CP_PROD_ID > 0) And (nISA_CP_PROD_ID <> itemId) Then
                            ' site id + inv_itemid FOUND BUT different commodity code compared to what's currently in ClassAvailableProducts table
                            sql = sqlUpdateCPJunctionViaPartNoIfNotEqual
                            sb = New System.Text.StringBuilder
                            sb.AppendFormat(sql, _
                                            oSite.SiteId, _
                                            invItemId, _
                                            itemId.ToString, _
                                            "Y", _
                                            "Y")
                            sql = sb.ToString
                            sb = Nothing
                            cmdOraMod = oraModCN.CreateCommand
                            cmdOraMod.CommandText = sql
                            cmdOraMod.CommandType = CommandType.Text
                            Dim nCount As Integer = cmdOraMod.ExecuteNonQuery
                            If (nCount > 0) Then
                                logger.WriteVerboseLog("site/item " & oSite.SiteId & " / " & invItemId & " FOUND in CP_JUNCTION but different item ID : UPDATING - old=" & nISA_CP_PROD_ID.ToString & ";new=" & itemId.ToString)
                            End If
                            Try
                                cmdOraMod.Dispose()
                            Catch ex As Exception
                            End Try
                            cmdOraMod = Nothing
                        End If
                    End If

                End While

                Try
                    oraModCN.Close()
                Catch ex As Exception
                End Try
                Try
                    oraModCN.Dispose()
                Catch ex As Exception
                End Try
                oraModCN = Nothing

            End If

            Try
                rdrClassAvailItems.Close()
            Catch ex As Exception
            End Try
            rdrClassAvailItems = Nothing

            Try
                cmdClassAvailItems.Dispose()
            Catch ex As Exception
            End Try
            cmdClassAvailItems = Nothing

        Else
            ' unable to open connection (oracle)
            logger.WriteErrorLog("unable to open connection : " & sql2012CN.DataSource)
        End If

        Try
            sql2012CN.Close()
            sql2012CN.Dispose()
        Catch ex As Exception
        End Try
        sql2012CN = Nothing

        sb = Nothing
        sr = Nothing

    End Sub

End Module
