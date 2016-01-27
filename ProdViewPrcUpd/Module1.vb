Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports System.Math
Imports System.IO
Imports SDI.ApplicationLogger


Module Module1

    Dim ORDB As String = "PROD"
    'Dim SQLDB As String = "CONTENTPLUS_JR"
    Dim SQLDB As String = "CPLUS_PROD"
    Dim SQLDBPROD As String = "DAZZLE"

    'Dim DBfile As String = "\\contentplus\c$\Program Files\Apache Tomcat 4.0\webapps\ContentPlus\WEB-INF\classes\com\eplus\contentplus\db.properties"

    'Dim DBfile As String = "\\contentplus_jr\c$\Program Files\Apache Software Foundation\Tomcat 5.5\webapps\ContentPlus\WEB-INF\classes\com\eplus\contentplus\db.properties"

    'Dim CplusDB As String
    Dim arrOROSites As ArrayList
    'Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\Program Files (x86)\SDI\ProdViewPrcUpd"
    Dim logpath As String = "C:\Program Files (x86)\SDI\ProdViewPrcUpd\LOGS\ProdViewPrcUpd" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    'Dim rootDir As String = "C:\ProdViewPrcUpd"
    'Dim logpath As String = "C:\ProdViewPrcUpd\LOGS\ProdViewPrcUpd" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Private m_logger As ApplicationLogger = Nothing


    Public Sub Main()

        Dim rtn As String = "Module1.Main"

        Console.WriteLine("Start ProdViewPrcUpd update")
        Console.WriteLine("")

        'If Dir(rootDir, FileAttribute.Directory) = "" Then
        '    MkDir(rootDir)
        'End If
        'If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
        '    MkDir(rootDir & "\LOGS")
        'End If

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Verbose

        ' params
        Dim param As Hashtable = ParseCommandParams(Command)
        If param.ContainsKey(key:="/LOG") Then
            Dim lvl As String = CStr(param(key:="/LOG")).Trim.ToUpper
            If lvl = "VERB" Or lvl = "VERBOSE" Then
                logLevel = TraceLevel.Verbose
            ElseIf lvl = "INFO" Or lvl = "INFORMATION" Then
                logLevel = TraceLevel.Info
            ElseIf lvl = "WARN" Or lvl = "WARNING" Then
                logLevel = TraceLevel.Warning
            ElseIf lvl = "ERR" Or lvl = "ERROR" Then
                logLevel = TraceLevel.Error
            ElseIf lvl = "OFF" Then
                logLevel = TraceLevel.Off
            Else
                ' don't change default
            End If
        End If
        If param.ContainsKey(key:="/DB") Then
            ORDB = CStr(param(key:="/DB")).Trim.ToUpper
            'Dim db As String = CStr(param(key:="/DB")).Trim.ToUpper
            'm_oraCNstring = "" & _
            '                oraCN_default_provider & _
            '                oraCN_default_creden & _
            '                "Data Source=" & db & ";" & _
            '                ""
        End If
        param = Nothing

        ' initialize log
        m_logger = New ApplicationLogger(logpath, logLevel)
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " v" & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")

        'objStreamWriter = File.CreateText(logpath)
        'objStreamWriter.WriteLine("Update of the item price  " & Now())

        Dim Z As Integer
        Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=" & ORDB & "")
        m_logger.WriteVerboseLog(rtn & " :: connection string (ORA) : [Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=" & ORDB & "]")

        Dim connectCplus As New SqlConnection("server=" & SQLDB & ";uid=einternet;pwd=einternet;Initial Catalog=ContentPlus;Data Source=" & SQLDB & "")
        m_logger.WriteVerboseLog(rtn & " :: connection string (SQL) : [server=" & SQLDB & ";uid=einternet;pwd=einternet;Initial Catalog=ContentPlus;Data Source=" & SQLDB & "]")

        Dim cn As SqlConnection = Nothing
        'Dim CplusDB As Hashtable
        'CplusDB = getCPlusdb(connectCplus)
        'm_logger.WriteVerboseLog(rtn & " :: SQL database count = " & CplusDB.Count.ToString)

        Dim CplusDB2012 As Hashtable
        CplusDB2012 = getCPlusdbSQL2012(connectCplus)
        m_logger.WriteVerboseLog(rtn & " :: SQL2012 database count = " & CplusDB2012.Count.ToString)

        Try
            connectOR.Open()
            m_logger.WriteVerboseLog(rtn & " :: connectOR (state) " & connectOR.State.ToString)
        Catch OLEDBExp As OleDbException
            
            m_logger.WriteErrorLog(rtn & " :: OLEDB SQL error - " & OLEDBExp.ToString)
        End Try

        'Dim Enumerator As IDictionaryEnumerator = CplusDB.GetEnumerator
        'Try

        '    connectCplus.Open()
        '    m_logger.WriteVerboseLog(rtn & " :: connectCplus (state) " & connectCplus.State.ToString)

        '    While (Enumerator.MoveNext())
        '        cn = Nothing
        '        'Enumerator.Value.open()
        '        Try
        '            cn = CType(Enumerator.Value, SqlConnection)
        '            cn.Open()
        '            m_logger.WriteVerboseLog(rtn & " :: " & cn.ConnectionString & " (state) " & cn.State.ToString)
        '        Catch ex As Exception
        '            m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
        '        End Try
        '    End While

        'Catch SQLExp As SqlException
        '    'objStreamWriter.WriteLine("")
        '    'objStreamWriter.WriteLine("***MS SQL error - " & SQLExp.ToString)
        '    'objStreamWriter.WriteLine("")
        '    m_logger.WriteErrorLog(rtn & " :: MS SQL error - " & SQLExp.ToString)
        'End Try

        Dim Enumerator2012 As IDictionaryEnumerator = CplusDB2012.GetEnumerator
        Try

            'connectCplus.Open()
            m_logger.WriteVerboseLog(rtn & " :: connectCplus (state) " & connectCplus.State.ToString)

            While (Enumerator2012.MoveNext())
                cn = Nothing

                Try
                    cn = CType(Enumerator2012.Value, SqlConnection)
                    cn.Open()
                    m_logger.WriteVerboseLog(rtn & " :: " & cn.ConnectionString & " (state) " & cn.State.ToString)
                Catch ex As Exception
                    m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
                End Try
            End While

        Catch SQLExp As SqlException

            m_logger.WriteErrorLog(rtn & " :: MS SQL2012 error - " & SQLExp.ToString)
        End Try

        connectOR.Close()
        m_logger.WriteVerboseLog(rtn & " :: connectOR (state) " & connectOR.State.ToString)
        connectCplus.Close()
        m_logger.WriteVerboseLog(rtn & " :: connectCplus (state) " & connectCplus.State.ToString)
        'Enumerator.Reset()
        'While (Enumerator.MoveNext())
        '    cn = Nothing
        '    'Enumerator.Value.close()
        '    Try
        '        cn = CType(Enumerator.Value, SqlConnection)
        '        If cn.State <> ConnectionState.Closed Then
        '            cn.Close()
        '        End If
        '        m_logger.WriteVerboseLog(rtn & " :: " & cn.ConnectionString & " (state) " & cn.State.ToString)
        '    Catch ex As Exception
        '        m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
        '    End Try
        'End While

        Enumerator2012.Reset()
        While (Enumerator2012.MoveNext())
            cn = Nothing

            Try
                cn = CType(Enumerator2012.Value, SqlConnection)
                If cn.State <> ConnectionState.Closed Then
                    cn.Close()
                End If
                m_logger.WriteVerboseLog(rtn & " :: " & cn.ConnectionString & " (state) " & cn.State.ToString)
            Catch ex As Exception
                m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
            End Try
        End While
        ' send our tracing messages
        'objStreamWriter.WriteLine("Program started at " & Now())

        Console.WriteLine("Open OR")
        connectOR.Open()
        m_logger.WriteVerboseLog(rtn & " :: connectOR (state) " & connectOR.State.ToString)

        Console.WriteLine("Open MS")
        'Enumerator = CplusDB.GetEnumerator
        'Try
        '    connectCplus.Open()
        '    m_logger.WriteVerboseLog(rtn & " :: connectCplus (state) " & connectCplus.State.ToString)

        '    While (Enumerator.MoveNext())
        '        cn = Nothing
        '        'Enumerator.Value.open()
        '        Try
        '            cn = CType(Enumerator.Value, SqlConnection)
        '            cn.Open()
        '            m_logger.WriteVerboseLog(rtn & " :: " & cn.ConnectionString & " (state) " & cn.State.ToString)
        '        Catch ex As Exception
        '            m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
        '        End Try
        '    End While

        '    'connectPub.Open()

        'Catch SQLExp As SqlException

        '    m_logger.WriteErrorLog(rtn & " :: MS SQL error - " & SQLExp.ToString)
        'End Try

        Enumerator2012 = CplusDB2012.GetEnumerator
        Try
            connectCplus.Open()
            m_logger.WriteVerboseLog(rtn & " :: connectCplus (state) " & connectCplus.State.ToString)

            While (Enumerator2012.MoveNext())
                cn = Nothing

                Try
                    cn = CType(Enumerator2012.Value, SqlConnection)
                    cn.Open()
                    m_logger.WriteVerboseLog(rtn & " :: " & cn.ConnectionString & " (state) " & cn.State.ToString)
                Catch ex As Exception
                    m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
                End Try
            End While

        Catch SQLExp As SqlException
            ''Catch the error and display it.

            m_logger.WriteErrorLog(rtn & " :: MS SQL2012 error - " & SQLExp.ToString)
        End Try

        Console.WriteLine("Provider: " & connectOR.Provider)
        Console.WriteLine("ServerVersion: " & connectOR.ServerVersion)
        Console.WriteLine("DataSource: " & connectOR.DataSource)
        Console.WriteLine()
        Console.WriteLine("Database: " & connectCplus.Database)
        Console.WriteLine("ServerVersion: " & connectCplus.ServerVersion)
        Console.WriteLine("DataSource: " & connectCplus.DataSource)
        Console.WriteLine()

        'Console.WriteLine("Database: " & connectPub.Database)
        'Console.WriteLine("ServerVersion: " & connectPub.ServerVersion)
        ' Console.WriteLine("DataSource: " & connectPub.DataSource)
        'Console.WriteLine()

        connectOR.Close()
        m_logger.WriteVerboseLog(rtn & " :: connectOR (state) " & connectOR.State.ToString)
        connectCplus.Close()
        m_logger.WriteVerboseLog(rtn & " :: connectCplus (state) " & connectCplus.State.ToString)
        'Enumerator.Reset()
        'While (Enumerator.MoveNext())
        '    Console.WriteLine("Database: " & Enumerator.Value.Database)
        '    Console.WriteLine("ServerVersion: " & Enumerator.Value.ServerVersion)
        '    Console.WriteLine("DataSource: " & Enumerator.Value.DataSource)
        '    Console.WriteLine()
        '    cn = Nothing
        '    'Enumerator.Value.close()
        '    Try
        '        cn = CType(Enumerator.Value, SqlConnection)
        '        If cn.State <> ConnectionState.Closed Then
        '            cn.Close()
        '        End If
        '        m_logger.WriteVerboseLog(rtn & " :: " & cn.ConnectionString & " (state) " & cn.State.ToString)
        '    Catch ex As Exception
        '        m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
        '    End Try
        'End While

        Enumerator2012.Reset()
        While (Enumerator2012.MoveNext())
            'Console.WriteLine("Database: " & Enumerator2012.Value.Database)
            'Console.WriteLine("ServerVersion: " & Enumerator2012.Value.ServerVersion)
            'Console.WriteLine("DataSource: " & Enumerator2012.Value.DataSource)
            'Console.WriteLine()
            cn = Nothing

            Try
                cn = CType(Enumerator2012.Value, SqlConnection)
                If cn.State <> ConnectionState.Closed Then
                    cn.Close()
                End If
                m_logger.WriteVerboseLog(rtn & " :: " & cn.ConnectionString & " (state) " & cn.State.ToString)
            Catch ex As Exception
                m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
            End Try
        End While

        Dim i As Integer
        Dim c As Char

        c = CChar("c")

        m_logger.WriteVerboseLog(rtn & " :: calling checktriggers() routine ... ")
        checktriggers()

        ''Console.ReadLine()
        'objStreamWriter.Close()

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try
    End Sub

    Public Sub OnChanged(ByVal source As Object, ByVal e As FileSystemEventArgs)
        Console.WriteLine("File: {0} {1}", e.FullPath, e.ChangeType.ToString("G"))
    End Sub

    'Private Sub checktriggers(ByVal source As Object, ByVal e As _
    'System.IO.FileSystemEventArgs)

    Private Sub checktriggers()
        Dim rtn As String = "Module1.checktriggers"
        't.Interval = 600000
        't.Enabled = False
        'tnow.Enabled = False

        Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=" & ORDB & "")
        'objStreamWriter.WriteLine("  Try Oracle connection open " & Now())
        'Trace.Flush()
        Try
            connectOR.Open()
            m_logger.WriteVerboseLog(rtn & " :: connectOR (state) " & connectOR.State.ToString)
        Catch OLEDBExp As OleDbException
            'objStreamWriter.WriteLine("")
            'objStreamWriter.WriteLine("***OLDDB Connection open error - " & OLEDBExp.ToString)
            'objStreamWriter.WriteLine("")
            m_logger.WriteErrorLog(rtn & " :: OLEDB Connection open error - " & OLEDBExp.ToString)
            Exit Sub
        End Try
        Dim connectCplus As New SqlConnection("server=" & SQLDB & ";uid=einternet;pwd=einternet;Initial Catalog=Contentplus")
        'Dim CplusDB As Hashtable = getCPlusdb(connectCplus)
        Dim CplusDB2012 As Hashtable
        CplusDB2012 = getCPlusdbSQL2012(connectCplus)
        'Dim Enumerator As IDictionaryEnumerator = CplusDB.GetEnumerator
        Dim Enumerator2012 As IDictionaryEnumerator = CplusDB2012.GetEnumerator
        Try

            'While (Enumerator.MoveNext())
            '    'Enumerator.Value.open()
            '    Try
            '        CType(Enumerator.Value, SqlConnection).Open()
            '        m_logger.WriteVerboseLog(rtn & " :: " & CType(Enumerator.Value, SqlConnection).ConnectionString & " (state) " & CType(Enumerator.Value, SqlConnection).State.ToString)
            '    Catch ex As Exception
            '        m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
            '    End Try
            'End While
        Catch SQLExp As SqlException
           
            m_logger.WriteErrorLog(rtn & " :: MS Connection open error - " & SQLExp.ToString)
            Exit Sub
        End Try

        Try
            connectCplus.Open()

            While (Enumerator2012.MoveNext())

                Try
                    CType(Enumerator2012.Value, SqlConnection).Open()
                    m_logger.WriteVerboseLog(rtn & " :: " & CType(Enumerator2012.Value, SqlConnection).ConnectionString & " (state) " & CType(Enumerator2012.Value, SqlConnection).State.ToString)
                Catch ex As Exception
                    m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
                End Try
            End While
        Catch SQLExp As SqlException
            ''Catch the error and display it.

            m_logger.WriteErrorLog(rtn & " :: MS2012 Connection open error - " & SQLExp.ToString)
            Exit Sub
        End Try
        
        m_logger.WriteVerboseLog(rtn & " :: Updated at " & Now.ToString)

        'because of conflicts with backups there is no update of the prices between 6:45 PM 
        ' and 4:00 AM

        If Now.TimeOfDay.TotalMinutes < 240 Or _
            Now.TimeOfDay.TotalMinutes > 1125 Then
            m_logger.WriteVerboseLog(rtn & " :: No Update at " & Now.ToString)
            Exit Sub
        End If
        Dim commandOR As New OleDbCommand("SELECT" & vbCrLf & _
            " A.ISA_SITE_ID," & vbCrLf & _
            " A.INV_ITEM_ID," & vbCrLf & _
            " A.ISA_CP_UNVRSAL_ID," & vbCrLf & _
            " A.ISA_CP_PROD_ID," & vbCrLf & _
            " A.ISA_CP_TRIG_FLAG," & vbCrLf & _
            " A.ISA_CP_TRIG_ON_DT," & vbCrLf & _
            " A.ISA_CP_TRIG_OFF_DT," & vbCrLf & _
            " B.INV_ITEM_TYPE," & vbCrLf & _
            " B.INV_STOCK_TYPE," & vbCrLf & _
            " B.RECYCLE_FLAG, C.PRICE " & vbCrLf & _
            " from ps_isa_cp_junction A, ps_inv_items B, SYSADM.PS_ISA_SDIEX_PRICE C " & vbCrLf & _
            " Where A.ISA_CP_TRIG_FLAG = 'Y'" & vbCrLf & _
            " AND A.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf & _
            " AND A.INV_ITEM_ID = C.INV_ITEM_ID" & vbCrLf & _
            "  AND B.EFFDT =" & vbCrLf & _
            " (SELECT MAX(B_ED.EFFDT) FROM PS_INV_ITEMS B_ED" & vbCrLf & _
            " WHERE(B.SETID = B_ED.SETID)" & vbCrLf & _
            " AND B.INV_ITEM_ID = B_ED.INV_ITEM_ID" & vbCrLf & _
            " AND B_ED.EFFDT <= SYSDATE)" & vbCrLf & _
            " ORDER BY A.INV_ITEM_ID" & vbCrLf & _
            "", connectOR)

        ' below statement was commented out on 11/11/03 becuase
        ' for whatever reason there were items that were inactive  
        ' and had a price change

        '" AND B.EFF_STATUS = 'A'" & vbCrLf & _

        Dim dataAdapter As New OleDbDataAdapter(commandOR)
        Dim ds As New DataSet
        'Dim cmdBuilder As OleDbCommandBuilder = _
        'New OleDbCommandBuilder(dataAdapter)
        dataAdapter.Fill(ds)

        'connectOR.Close()
        Dim I As Integer
        Dim X As Integer
        Dim intArrCnt As Integer
        Dim bPrcUpd As Boolean
        Dim sCPTrigFlg As String
        Dim iRecsUpd As Integer = ds.Tables(0).Rows.Count
        Dim strInvItemID As String = ""
        Dim arrSites As ArrayList
        Dim strPrevSiteID As String = " "
        Dim hshClassids As Hashtable = buildproductviewclassid(connectCplus)
        Dim connectPub As SqlConnection
        Dim connectPub2012 As SqlConnection
        Dim strClassid
        Dim strItemMode As String

        'Dim EnumClassids As IDictionaryEnumerator = CplusDB.GetEnumerator

        arrOROSites = CheckforOROSites(connectOR)

        For I = 0 To ds.Tables(0).Rows.Count - 1
            If Not Convert.ToString(ds.Tables(0).Rows(I).Item(0)).Substring(0, 3) = strPrevSiteID Then

                arrSites = buildSitesArray(ds.Tables(0).Rows(I).Item(0), connectOR)
                strItemMode = getItemMode(ds.Tables(0).Rows(I).Item(0), connectOR)
                strPrevSiteID = Convert.ToString(ds.Tables(0).Rows(I).Item(0)).Substring(0, 3)
            End If

            For X = 0 To arrSites.Count - 1
                If ds.Tables(0).Rows(I).Item(0) = "ORO" Then
                    strInvItemID = ds.Tables(0).Rows(I).Item(1)
                Else
                    If strItemMode = "4" Or strItemMode = "6" Then
                        strInvItemID = ds.Tables(0).Rows(I).Item(1)
                    Else
                        strInvItemID = ds.Tables(0).Rows(I).Item(1).SubString(3)
                    End If
                End If

                'Dim itmPrice = FormatNumber(GetPrice(Convert.ToString(arrSites(X)).Substring(0, 3), _
                '    ds.Tables(0).Rows(I).Item(1), _
                '    ds.Tables(0).Rows(I).Item(7), _
                '    ds.Tables(0).Rows(I).Item(8), _
                '    connectOR))
                Dim itmPrice As Decimal = ds.Tables(0).Rows(I).Item(10)
                Dim strBu3 As String = Convert.ToString(arrSites(X)).Substring(0, 3)
                'itmPrice = GetSDiExPrice1(strInvItemID, strBu3)
                'itmPrice = GetPrice(sSite:=Convert.ToString(arrSites(X)).Substring(0, 3), _
                '                    sprodid:=ds.Tables(0).Rows(I).Item(1), _
                '                    sitmtyp:=ds.Tables(0).Rows(I).Item(7), _
                '                    sstktyp:=ds.Tables(0).Rows(I).Item(8), _
                '                    connectOR:=connectOR)
                If itmPrice > 0 Then
                    Dim mySql1 As String = "UPDATE  classavailableproducts" & _
                        " SET promoprice = cast('" & itmPrice & "' as money)," & _
                        " promoeffectivedate = getdate()," & _
                        " promoexpirydate = '09-SEP-9999'," & _
                        " upddate = getdate()," & _
                        " upduser = 'vbjunc'" & _
                        " Where itemid = '" & ds.Tables(0).Rows(I).Item(3) & "'" & vbCrLf & _
                        " AND productviewid = " & Convert.ToString(arrSites(X)).Substring(3) & vbCrLf & _
                        " AND customeritemid = '" & strInvItemID & "'"
                    If ds.Tables(0).Rows(I).Item(0) = "ORO" Then
                        Dim commandMS1 As New SqlCommand(mySql1, connectCplus)
                        bPrcUpd = True
                        Try
                            Dim rowsaffected As Integer = commandMS1.ExecuteNonQuery()
                            If rowsaffected = 0 Then
                                bPrcUpd = False
                            End If

                        Catch SQLExp As SqlException
                            m_logger.WriteErrorLog(rtn & " :: MS SQL error - " & SQLExp.ToString & " ::: " & mySql1)
                            Exit Sub
                        Finally
                            'Do the final cleanup such as closing the Database connection
                        End Try
                        strClassid = hshClassids(Convert.ToString(arrSites(X)).Substring(3))
                        'Enumerator.Reset()
                        'While (Enumerator.MoveNext())
                        '    If Convert.ToString(Enumerator.Key).Substring(0, 6) = strClassid Then
                        '        connectPub = CplusDB(Enumerator.Key)
                        '    End If
                        'End While

                        'Dim commandMS2 As New SqlCommand("UPDATE  classavailableproducts" & _
                        '    " SET promoprice = cast('" & itmPrice & "' as money)," & _
                        '    " promoeffectivedate = getdate()," & _
                        '    " promoexpirydate = '09-SEP-9999'," & _
                        '    " upddate = getdate()," & _
                        '    " upduser = 'vbjunc'" & _
                        '    " Where itemid = '" & ds.Tables(0).Rows(I).Item(3) & "'" & vbCrLf & _
                        '    " AND productviewid = " & Convert.ToString(arrSites(X)).Substring(3) & vbCrLf & _
                        '    " AND customeritemid = '" & strInvItemID & "'", connectPub)
                        'bPrcUpd = True
                        'Try
                        '    Dim rowsaffected As Integer = commandMS2.ExecuteNonQuery()
                        '    If rowsaffected = 0 Then
                        '        bPrcUpd = False
                        '        'objStreamWriter.WriteLine("FALSE-" & connectPub.Database & " " & ds.Tables(0).Rows(I).Item(0) _
                        '        '& " " & ds.Tables(0).Rows(I).Item(1) _
                        '        '& " " & ds.Tables(0).Rows(I).Item(3) _
                        '        '& " " & Convert.ToString(arrSites(X)).Substring(3) _
                        '        '& " - " & itmPrice)
                        '        m_logger.WriteVerboseLog(rtn & " :: FALSE-" & connectPub.Database & " " & ds.Tables(0).Rows(I).Item(0) _
                        '                                     & " " & ds.Tables(0).Rows(I).Item(1) _
                        '                                     & " " & ds.Tables(0).Rows(I).Item(3) _
                        '                                     & " " & Convert.ToString(arrSites(X)).Substring(3) _
                        '                                     & " - " & itmPrice)
                        '    End If

                        'Catch SQLExp As SqlException
                        '    'Catch the error and display it.
                        '    'objStreamWriter.WriteLine("   MS SQL Record not Found - " & ds.Tables(0).Rows(I).Item(1))
                        '    'objStreamWriter.WriteLine("")
                        '    'objStreamWriter.WriteLine("***MS SQL error - " & SQLExp.ToString)
                        '    'objStreamWriter.WriteLine("")
                        '    m_logger.WriteErrorLog(rtn & " :: MS SQL error - " & SQLExp.ToString)
                        '    Exit Sub
                        'Finally
                        '    'Do the final cleanup such as closing the Database connection
                        'End Try

                        Enumerator2012.Reset()
                        While (Enumerator2012.MoveNext())
                            If Convert.ToString(Enumerator2012.Key).Substring(0, 6) = strClassid Then
                                connectPub2012 = CplusDB2012(Enumerator2012.Key)
                            End If
                        End While

                        If Not connectPub2012 Is Nothing Then
                            Dim commandMS21 As New SqlCommand(mySql1, connectPub2012)
                            bPrcUpd = True
                            Try
                                Dim rowsaffected As Integer = commandMS21.ExecuteNonQuery()
                                If rowsaffected = 0 Then
                                    bPrcUpd = False
                                    m_logger.WriteVerboseLog(rtn & " :: FALSE-" & connectPub2012.Database & " " & ds.Tables(0).Rows(I).Item(0) _
                                                                 & " " & ds.Tables(0).Rows(I).Item(1) _
                                                                 & " " & ds.Tables(0).Rows(I).Item(3) _
                                                                 & " " & Convert.ToString(arrSites(X)).Substring(3) _
                                                                 & " - " & itmPrice)
                                End If

                            Catch SQLExp As SqlException
                                m_logger.WriteErrorLog(rtn & " :: MS2012 SQL error - " & SQLExp.ToString & " ::: " & mySql1)
                                'Exit Sub
                            Finally
                                'Do the final cleanup such as closing the Database connection
                            End Try
                        End If

                    Else
                        Dim commandMS1 As New SqlCommand(mySql1, connectCplus)
                        bPrcUpd = True
                        Try
                            Dim rowsaffected As Integer = commandMS1.ExecuteNonQuery()
                            If rowsaffected = 0 Then
                                bPrcUpd = False
                            End If

                        Catch SQLExp As SqlException
                            m_logger.WriteErrorLog(rtn & " :: MS SQL error - " & sqlexp.ToString & " ::: " & mySql1)
                            'Exit Sub
                        Finally
                            'Do the final cleanup such as closing the Database connection
                        End Try

                        strClassid = hshClassids(Convert.ToString(arrSites(X)).Substring(3))
                        'Enumerator.Reset()
                        'While (Enumerator.MoveNext())
                        '    If Convert.ToString(Enumerator.Key).Substring(0, 6) = strClassid Then
                        '        connectPub = CplusDB(Enumerator.Key)
                        '    End If
                        'End While
                        'Dim commandMS2 As New SqlCommand("UPDATE  classavailableproducts" & _
                        '    " SET promoprice = cast('" & itmPrice & "' as money)," & _
                        '    " promoeffectivedate = getdate()," & _
                        '    " promoexpirydate = dateadd(year, 20, getdate())," & _
                        '    " upddate = getdate()," & _
                        '    " upduser = 'vbjunc'" & _
                        '    " Where itemid = '" & ds.Tables(0).Rows(I).Item(3) & "'" & vbCrLf & _
                        '    " AND productviewid = " & Convert.ToString(arrSites(X)).Substring(3) & vbCrLf & _
                        '    " AND customeritemid = '" & strInvItemID & "'", connectPub)
                        'bPrcUpd = True
                        'Try
                        '    Dim rowsaffected As Integer = commandMS2.ExecuteNonQuery()
                        '    If rowsaffected = 0 Then
                        '        bPrcUpd = False
                        '        'objStreamWriter.WriteLine("FALSE-" & connectPub.Database & " " & ds.Tables(0).Rows(I).Item(0) _
                        '        '& " " & ds.Tables(0).Rows(I).Item(1) _
                        '        '& " " & ds.Tables(0).Rows(I).Item(3) _
                        '        '& " " & Convert.ToString(arrSites(X)).Substring(3) _
                        '        '& " - " & itmPrice)
                        '        m_logger.WriteVerboseLog(rtn & " :: FALSE-" & connectPub.Database & " " & ds.Tables(0).Rows(I).Item(0) _
                        '                                     & " " & ds.Tables(0).Rows(I).Item(1) _
                        '                                     & " " & ds.Tables(0).Rows(I).Item(3) _
                        '                                     & " " & Convert.ToString(arrSites(X)).Substring(3) _
                        '                                     & " - " & itmPrice)
                        '    End If

                        'Catch SQLExp As SqlException
                        '    ''Catch the error and display it.
                        '    ''objStreamWriter.WriteLine("   MS SQL Record not Found - " & ds.Tables(0).Rows(I).Item(1))
                        '    'objStreamWriter.WriteLine("")
                        '    'objStreamWriter.WriteLine("***MS SQL error - " & SQLExp.ToString)
                        '    'objStreamWriter.WriteLine("")
                        '    m_logger.WriteErrorLog(rtn & " :: MS SQL error - " & sqlexp.ToString)
                        '    Exit Sub
                        'Finally
                        '    'Do the final cleanup such as closing the Database connection
                        'End Try

                        Enumerator2012.Reset()
                        While (Enumerator2012.MoveNext())
                            If Convert.ToString(Enumerator2012.Key).Substring(0, 6) = strClassid Then
                                connectPub2012 = CplusDB2012(Enumerator2012.Key)
                            End If
                        End While
                        If Not connectPub2012 Is Nothing Then
                            mySql1 = "UPDATE  classavailableproducts" & _
                                " SET promoprice = cast('" & itmPrice & "' as money)," & _
                                " promoeffectivedate = getdate()," & _
                                " promoexpirydate = dateadd(year, 20, getdate())," & _
                                " upddate = getdate()," & _
                                " upduser = 'vbjunc'" & _
                                " Where itemid = '" & ds.Tables(0).Rows(I).Item(3) & "'" & vbCrLf & _
                                " AND productviewid = " & Convert.ToString(arrSites(X)).Substring(3) & vbCrLf & _
                                " AND customeritemid = '" & strInvItemID & "'"
                            Dim commandMS22 As New SqlCommand(mySql1, connectPub2012)
                            bPrcUpd = True
                            Try
                                Dim rowsaffected As Integer = commandMS22.ExecuteNonQuery()
                                If rowsaffected = 0 Then
                                    bPrcUpd = False
                                    m_logger.WriteVerboseLog(rtn & " :: FALSE-" & connectPub2012.Database & " " & ds.Tables(0).Rows(I).Item(0) _
                                                                 & " " & ds.Tables(0).Rows(I).Item(1) _
                                                                 & " " & ds.Tables(0).Rows(I).Item(3) _
                                                                 & " " & Convert.ToString(arrSites(X)).Substring(3) _
                                                                 & " - " & itmPrice)
                                End If

                            Catch SQLExp As SqlException
                                m_logger.WriteErrorLog(rtn & " :: MS2012 SQL error - " & sqlexp.ToString & " ::: " & mySql1)
                                'Exit Sub
                            Finally
                                'Do the final cleanup such as closing the Database connection
                            End Try
                        End If

                    End If
                Else
                    bPrcUpd = True
                End If

                If bPrcUpd = True Then
                    sCPTrigFlg = " "
                    'objStreamWriter.WriteLine("   " & ds.Tables(0).Rows(I).Item(0) _
                    '    & " " & ds.Tables(0).Rows(I).Item(1) _
                    '    & " " & ds.Tables(0).Rows(I).Item(3) _
                    '    & " - " & itmPrice)
                    m_logger.WriteVerboseLog(rtn & "   " & ds.Tables(0).Rows(I).Item(0) _
                                                 & " " & ds.Tables(0).Rows(I).Item(1) _
                                                 & " " & ds.Tables(0).Rows(I).Item(3) _
                                                 & " - " & itmPrice)
                Else
                    If ds.Tables(0).Rows(I).Item(0) = "ORO" Then
                        sCPTrigFlg = " "
                    Else
                        sCPTrigFlg = "E"
                    End If
                    iRecsUpd = iRecsUpd - 1
                End If
                If X = arrSites.Count - 1 Then
                    Dim commandORUpd As New OleDbCommand("UPDATE ps_isa_cp_junction " & _
                         " SET ISA_CP_TRIG_OFF_DT = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," & _
                         " ISA_CP_TRIG_FLAG = '" & sCPTrigFlg & "'" & _
                         " Where ISA_SITE_ID = '" & ds.Tables(0).Rows(I).Item(0) & "'" & vbCrLf & _
                         " AND INV_ITEM_ID = '" & ds.Tables(0).Rows(I).Item(1) & "'", connectOR)

                    Try
                        Dim rowsaffected As Integer = commandORUpd.ExecuteNonQuery()

                    Catch oledbExp As OleDbException
                        m_logger.WriteErrorLog(rtn & " :: oledb SQL error - " & oledbExp.ToString)
                    Finally
                        'connectOR.Close()
                    End Try
                End If
                Console.WriteLine(ds.Tables(0).Rows(I).Item(0) _
                    & " " & ds.Tables(0).Rows(I).Item(1) _
                    & " " & ds.Tables(0).Rows(I).Item(4) _
                    & " " & ds.Tables(0).Rows(I).Item(6) _
                    & " " & sCPTrigFlg)
            Next
            'because of conflicts with backups there is no update of the prices between 6:45 PM 
            ' and 4:00 AM

            If Now.TimeOfDay.TotalMinutes < 240 Or _
                Now.TimeOfDay.TotalMinutes > 1125 Then
                'objStreamWriter.WriteLine("No Update at " & Now())
                m_logger.WriteVerboseLog(rtn & " :: No Update at " & Now.ToString)
                Trace.Flush()
                Exit Sub
            End If
        Next
        connectOR.Close()
        m_logger.WriteVerboseLog(rtn & " :: connectOR (state) " & connectOR.State.ToString)
        connectCplus.Close()
        m_logger.WriteVerboseLog(rtn & " :: connectCplus (state) " & connectCplus.State.ToString)
        'Enumerator.Reset()
        'While (Enumerator.MoveNext())
        '    'Enumerator.Value.close()
        '    Try
        '        CType(Enumerator.Value, SqlConnection).Close()
        '        m_logger.WriteVerboseLog(rtn & " :: " & CType(Enumerator.Value, SqlConnection).ConnectionString & " (state) " & CType(Enumerator.Value, SqlConnection).State.ToString)
        '    Catch ex As Exception
        '        m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
        '    End Try
        'End While

        Enumerator2012.Reset()
        While (Enumerator2012.MoveNext())

            Try
                CType(Enumerator2012.Value, SqlConnection).Close()
                m_logger.WriteVerboseLog(rtn & " :: " & CType(Enumerator2012.Value, SqlConnection).ConnectionString & " (state) " & CType(Enumerator2012.Value, SqlConnection).State.ToString)
            Catch ex As Exception
                m_logger.WriteErrorLog(rtn & " :: " & ex.ToString)
            End Try
        End While

        ' Update ps_isa_cp_junction with modified data
        'dataAdapter.UpdateCommand = cmdBuilder.GetUpdateCommand()
        'dataAdapter.Update(ds.Tables(0))

        'objStreamWriter.WriteLine("   Total records with trigger set on = " & iRecsUpd)
        m_logger.WriteVerboseLog(rtn & " :: Total records with trigger set on = " & iRecsUpd)

        ' flush the listners buffer and close 
        ' the output
        Trace.Flush()
        'traceLog.Close()
        Dim FilePath As String = "vbCPJuncUpd.txt"

        File.Delete(FilePath)
        Console.WriteLine("Waiting for timer" & Now.TimeOfDay.ToString)

        'Thread.Sleep(10500)
        't.Interval = 600000
        't.Enabled = True

    End Sub

    Function buildproductviewclassid(ByVal connectCplus As SqlConnection) As Hashtable

        Dim I As Integer
        Dim hshClassids As New Hashtable
        Dim commandMSRD As New SqlCommand("SELECT PRODUCTVIEWID, CLASSID" & _
                        " FROM PRODUCTVIEWS", connectCplus)

        Dim dataAdapter As New SqlDataAdapter(commandMSRD)
        Dim ds As New DataSet
        'Dim cmdBuilder As OleDbCommandBuilder = _
        'New OleDbCommandBuilder(dataAdapter)
        dataAdapter.Fill(ds)
        For I = 0 To ds.Tables(0).Rows.Count - 1
            hshClassids.Add(Convert.ToString(ds.Tables(0).Rows(I).Item("PRODUCTVIEWID")), ds.Tables(0).Rows(I).Item("CLASSID"))
        Next
        Return hshClassids

    End Function

    Function buildSitesArray(ByVal strSiteID As String, ByVal connectOR As OleDbConnection) As ArrayList

        Dim I As Integer
        Dim arrSites As ArrayList
        arrSites = New ArrayList

        If strSiteID = "ORO" Then
            arrSites = arrOROSites
            Return arrSites
        End If

        Dim commandOR As New OleDbCommand("SELECT" & vbCrLf & _
            " substr(A.ISA_BUSINESS_UNIT,3,3) as sitecode," & vbCrLf & _
            " A.ISA_CPLUS_PRODVIEW" & vbCrLf & _
            " FROM PS_ISA_ENTERPRISE A" & vbCrLf & _
            " WHERE A.ISA_BUSINESS_UNIT = 'I0" & strSiteID & "'" & vbCrLf & _
            " ", connectOR)

        Dim dataAdapter As New OleDbDataAdapter(commandOR)
        Dim ds As New DataSet
        dataAdapter.Fill(ds)
        For I = 0 To ds.Tables(0).Rows.Count - 1
            arrSites.Add(ds.Tables(0).Rows(I).Item(0) & ds.Tables(0).Rows(I).Item(1))
        Next
        Return arrSites

    End Function

    Function CheckforOROSites(ByVal connectOR As OleDb.OleDbConnection) As ArrayList
        Dim I As Integer
        Dim strSQLstring As String
        Dim arrSites As ArrayList

        arrSites = New ArrayList

        Dim commandOR As New OleDbCommand("SELECT" & vbCrLf & _
                    " substr(A.ISA_BUSINESS_UNIT,3,3) as sitecode," & vbCrLf & _
                    " A.ISA_CPLUS_PRODVIEW" & vbCrLf & _
                    " FROM PS_ISA_ENTERPRISE A" & vbCrLf & _
                    " WHERE A.ISA_CPLUS_PRODVIEW > '0'", connectOR)

        Dim dataAdapter As New OleDbDataAdapter(commandOR)
        Dim ds As New DataSet
        dataAdapter.Fill(ds)
        For I = 0 To ds.Tables(0).Rows.Count - 1
            Select Case ds.Tables(0).Rows(I).Item(0)
                'Case "242", "230", "239"
                '    arrSites.Add(ds.Tables(0).Rows(I).Item(0) & ds.Tables(0).Rows(I).Item(1))
            End Select
        Next
        Return arrSites

    End Function

    Function getItemMode(ByVal strSiteID As String, ByVal connectOR As OleDbConnection) As String

        If strSiteID = "ORO" Then
            getItemMode = "5"
            Return getItemMode
        End If

        Dim commandOR As New OleDbCommand("SELECT" & vbCrLf & _
            " ISA_ITEMID_MODE" & vbCrLf & _
            " FROM PS_ISA_ENTERPRISE A" & vbCrLf & _
            " WHERE A.ISA_BUSINESS_UNIT = 'I0" & strSiteID & "'" & vbCrLf & _
            " ", connectOR)

        getItemMode = commandOR.ExecuteScalar

        Return getItemMode

    End Function

    Function GetPrice(ByVal sSite As String, _
                    ByVal sprodid As String, _
                    ByVal sitmtyp As String, _
                    ByVal sstktyp As String, _
                    ByVal connectOR As OleDbConnection) As Decimal

        ' 2008.11.19
        '   taking OFF all round method call since item exists which price is lower than a cent
        '   - erwin

        ' Here is how we calculate price:
        '   If there is a fixed price for the item (meaning there is a active record in the
        '	PROD_PRICE table for the item, business unit I then C ORO and blank BU) we always use the fixed price.
        '	If NOT fixed price then if the item is in the customer inventory (C,P,R) and
        '	the regrind flag is set to yes, we get the last vendor price paid from the ITM_VNDR_UOM_PR
        '	table plus the markup from the ISA_PRICE_RULE table (using the I business unit).
        '	If SDI inventory (I,B) or customer inventory (C,P,R) and NOT regrind then we get the TL_COST
        '	from the CM_PRODCOST table plus the markup from the ISA_PRICE_RULE table.  For the customer
        '	inventory we will never find a markup in the ISA_PRICE_RULE table, but that is OK because
        '	the TL_COST already contains the markup for the customer inventory.

        'Dim decCPrice As Decimal = FormatCurrency(Round(0, 2), 2)
        Dim decCPrice As Decimal = CDec("0")
        'Dim decIPrice As Decimal = FormatCurrency(Round(0, 2), 2)
        Dim decIPrice As Decimal = CDec("0")

        Dim bFixedPrice As Boolean = False
        Dim bAvgPrice As Boolean = False
        Dim bVndPrice As Boolean = False

        connectOR.Close()
        connectOR.Open()

        ' get fixed price

        Dim commandOR1 As New OleDbCommand("SELECT A.SETID, A.PRODUCT_ID, A.BUSINESS_UNIT_IN, A.LIST_PRICE" & vbCrLf & _
             " FROM PS_PROD_PRICE A" & vbCrLf & _
             " WHERE A.SETID = 'MAIN1'" & vbCrLf & _
             " AND SUBSTR(A.BUSINESS_UNIT_IN,3,3) = '" & sSite & "'" & vbCrLf & _
             " AND A.PRODUCT_ID = '" & sprodid & "'" & vbCrLf & _
             " AND A.EFFDT =" & vbCrLf & _
             " (SELECT MAX(A_ED.EFFDT) FROM PS_PROD_PRICE A_ED" & vbCrLf & _
             "  WHERE A.SETID = A_ED.SETID" & vbCrLf & _
             "  AND A.PRODUCT_ID = A_ED.PRODUCT_ID" & vbCrLf & _
             "  AND A.UNIT_OF_MEASURE = A_ED.UNIT_OF_MEASURE" & vbCrLf & _
             "  AND A.BUSINESS_UNIT_IN = A_ED.BUSINESS_UNIT_IN" & vbCrLf & _
             "  AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
             " AND A.EFF_STATUS = 'A'", connectOR)

        Dim readerOR1 As OleDbDataReader = _
                      commandOR1.ExecuteReader

        While readerOR1.Read
            'Console.WriteLine("BU = " & readerOR.GetValue(2).SubString(0, 1))
            If readerOR1.GetValue(2).SubString(0, 1) = "C" Then
                'decCPrice = FormatNumber(Round(readerOR1.GetValue(3), 2), 2)
                Try
                    decCPrice = CDec(readerOR1.GetValue(3))
                Catch ex As Exception
                End Try
                bFixedPrice = True
            ElseIf readerOR1.GetValue(2).SubString(0, 1) = "I" Then
                'decIPrice = FormatNumber(Round(readerOR1.GetValue(3), 2), 2)
                Try
                    decIPrice = CDec(readerOR1.GetValue(3))
                Catch ex As Exception
                End Try
                bFixedPrice = True
            End If
        End While
        readerOR1.Close()
        'connectOR.Close()
        If bFixedPrice = True Then
            If decIPrice > 0.0 Then
                Return decIPrice
            Else
                Return decCPrice
            End If
        End If

        ' If SDI ORO item check for vendor price

        If sstktyp = "ORO" Then
            'connectOR.Open()
            Dim commandOR3A As New OleDbCommand("SELECT A.SETID, A.PRODUCT_ID, A.BUSINESS_UNIT_IN, A.LIST_PRICE" & vbCrLf & _
             " FROM PS_PROD_PRICE A" & vbCrLf & _
             " WHERE A.SETID = 'MAIN1'" & vbCrLf & _
             " AND A.BUSINESS_UNIT_IN = ' '" & vbCrLf & _
             " AND A.PRODUCT_ID = '" & sprodid & "'" & vbCrLf & _
             " AND A.EFFDT =" & vbCrLf & _
             " (SELECT MAX(A_ED.EFFDT) FROM PS_PROD_PRICE A_ED" & vbCrLf & _
             "  WHERE A.SETID = A_ED.SETID" & vbCrLf & _
             "  AND A.PRODUCT_ID = A_ED.PRODUCT_ID" & vbCrLf & _
             "  AND A.UNIT_OF_MEASURE = A_ED.UNIT_OF_MEASURE" & vbCrLf & _
             "  AND A.BUSINESS_UNIT_IN = A_ED.BUSINESS_UNIT_IN" & vbCrLf & _
             "  AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
             " AND A.EFF_STATUS = 'A'", connectOR)

            Dim readerOR3A As OleDbDataReader = _
                          commandOR3A.ExecuteReader

            While readerOR3A.Read
                'decIPrice = FormatNumber(Round(readerOR3A.GetValue(3), 2), 2)
                Try
                    decIPrice = CDec(readerOR3A.GetValue(3))
                Catch ex As Exception
                End Try
                bFixedPrice = True
            End While
            readerOR3A.Close()
            'connectOR.Close()
            If bFixedPrice = True Then
                If decIPrice > 0.0 Then
                    Return decIPrice
                End If
            End If

            Dim commandOR3 As New OleDbCommand("SELECT A.PRICE_VNDR" & vbCrLf & _
            " FROM PS_ITM_VNDR_UOM_PR A" & vbCrLf & _
            " WHERE A.SETID = 'MAIN1'" & vbCrLf & _
            " AND A.INV_ITEM_ID = '" & sprodid & "'" & vbCrLf & _
            " AND A.EFF_STATUS = 'A'" & vbCrLf & _
            " AND A.EFFDT =" & vbCrLf & _
            " (SELECT MAX(A_ED.EFFDT) FROM PS_ITM_VNDR_UOM_PR A_ED" & vbCrLf & _
            "  WHERE A.SETID = A_ED.SETID" & vbCrLf & _
            "  AND A.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf & _
            "  AND A.VENDOR_SETID = A_ED.VENDOR_SETID" & vbCrLf & _
            "  AND A.VNDR_LOC = A_ED.VNDR_LOC" & vbCrLf & _
            "  AND A.UNIT_OF_MEASURE = A_ED.UNIT_OF_MEASURE" & vbCrLf & _
            "  AND A.CURRENCY_CD = A_ED.CURRENCY_CD" & vbCrLf & _
            "  AND A.QTY_MIN = A_ED.QTY_MIN)", connectOR)

            'Console.WriteLine(commandOR3.CommandText)
            Dim readerOR3 As OleDbDataReader = _
                          commandOR3.ExecuteReader

            While readerOR3.Read
                'decIPrice = FormatNumber(Round(readerOR3.GetValue(0), 2), 2)
                Try
                    decIPrice = CDec(readerOR3.GetValue(0))
                Catch ex As Exception
                End Try
                bVndPrice = True
            End While
            readerOR3.Close()
            'connectOR.Close()
            If bVndPrice = True Then
                If decIPrice > 0.0 Then
                    Return getmarkup(decIPrice, sSite, sitmtyp, sstktyp, connectOR)
                Else
                    Return decIPrice
                End If
            End If

            'connectOR.Close()

        End If

        connectOR.Close()
        connectOR.Open()

        Dim orderGrpList As String = "'" & sstktyp & "'"
        If sstktyp = "STK" Then
            orderGrpList &= ",'CSTK'"
        End If

        'Dim commandOR2 As New OleDbCommand("SELECT" & vbCrLf & _
        '     " A.BUSINESS_UNIT," & vbCrLf & _
        '     " A.TL_COST," & vbCrLf & _
        '     " B.ISA_MARKUP_RATE" & vbCrLf & _
        '     " FROM PS_CM_PRODCOST A, PS_ISA_PRICE_RULE B" & vbCrLf & _
        '     " WHERE A.INV_ITEM_ID = '" & sprodid & "'" & vbCrLf & _
        '     " AND SUBSTR(A.BUSINESS_UNIT,3,3) = '" & sSite & "'" & vbCrLf & _
        '     " AND A.EFFDT =" & vbCrLf & _
        '     " (SELECT MAX(A_ED.EFFDT) FROM PS_CM_PRODCOST A_ED" & vbCrLf & _
        '     "  WHERE A.BUSINESS_UNIT = A_ED.BUSINESS_UNIT" & vbCrLf & _
        '     "  AND A.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf & _
        '     "  AND A.CONFIG_CODE = A_ED.CONFIG_CODE" & vbCrLf & _
        '     "  AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
        '     "  AND A.BUSINESS_UNIT = B.BUSINESS_UNIT(+)" & vbCrLf & _
        '     "  AND B.INV_ITEM_TYPE(+) = '" & sitmtyp & "'" & vbCrLf & _
        '     "  AND B.ORDER_GRP(+) = '" & sstktyp & "'", connectOR)
        Dim sql As String = "" & _
                            "SELECT" & vbCrLf & _
                            " A.BUSINESS_UNIT " & vbCrLf & _
                            ",A.TL_COST" & vbCrLf & _
                            ",B.ISA_MARKUP_RATE" & vbCrLf & _
                            "FROM PS_CM_PRODCOST A" & vbCrLf & _
                            ",(" & vbCrLf & _
                            "  SELECT " & vbCrLf & _
                            "   B1.BUSINESS_UNIT" & vbCrLf & _
                            "  ,B1.SHIP_FROM_BU" & vbCrLf & _
                            "  ,B1.ORDER_GRP" & vbCrLf & _
                            "  ,B1.INV_ITEM_TYPE" & vbCrLf & _
                            "  ,B1.ISA_MARKUP_RATE" & vbCrLf & _
                            "  FROM PS_ISA_PRICE_RULE B1" & vbCrLf & _
                            "  WHERE SUBSTR(B1.BUSINESS_UNIT,3,3) = '" & sSite & "' " & vbCrLf & _
                            "    AND B1.INV_ITEM_TYPE = '" & sitmtyp & "' " & vbCrLf & _
                            "    AND B1.ORDER_GRP IN (" & orderGrpList & ") " & vbCrLf & _
                            " ) B" & vbCrLf & _
                            "WHERE SUBSTR(A.BUSINESS_UNIT,3,3) = '" & sSite & "' " & vbCrLf & _
                            "  AND A.EFFDT = (SELECT MAX(A_ED.EFFDT) FROM PS_CM_PRODCOST A_ED" & vbCrLf & _
                            "                 WHERE A.BUSINESS_UNIT = A_ED.BUSINESS_UNIT" & vbCrLf & _
                            "                   AND A.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf & _
                            "                   AND A.CONFIG_CODE = A_ED.CONFIG_CODE" & vbCrLf & _
                            "                   AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                            "  AND A.BUSINESS_UNIT = B.SHIP_FROM_BU (+)" & vbCrLf & _
                            "  AND A.INV_ITEM_ID = '" & sprodid & "' " & vbCrLf & _
                            ""
        Dim commandOR2 As New OleDbCommand(sql, connectOR)

        'Console.WriteLine(commandOR2.CommandText)
        Dim readerOR2 As OleDbDataReader = _
                      commandOR2.ExecuteReader

        While readerOR2.Read
            'Console.WriteLine("BU = " & readerOR2.GetValue(0).SubString(0, 1))
            If readerOR2.GetValue(0).SubString(0, 1) = "C" Then
                If IsDBNull(readerOR2.GetValue(2)) Then
                    'decCPrice = FormatNumber(Round(readerOR2.GetValue(1), 2), 2)
                    ''Console.WriteLine("cprice1 = " & decCPrice)
                    Try
                        decCPrice = CDec(readerOR2.GetValue(1))
                    Catch ex As Exception
                    End Try
                Else
                    'decCPrice = FormatNumber(Round(CDbl(readerOR2.GetValue(1)) + (CDbl(readerOR2.GetValue(1)) * (CDbl(readerOR2.GetValue(2)) / 100)), 2), 2)
                    ''Console.WriteLine("cprice2 = " & decCPrice)
                    Try
                        decCPrice = CDec(CDbl(readerOR2.GetValue(1)) + (CDbl(readerOR2.GetValue(1)) * (CDbl(readerOR2.GetValue(2)) / 100)))
                    Catch ex As Exception
                    End Try
                End If
                bAvgPrice = True
            ElseIf readerOR2.GetValue(0).SubString(0, 1) = "P" And decCPrice = 0.0 Then
                If IsDBNull(readerOR2.GetValue(2)) Then
                    'decCPrice = FormatNumber(Round(readerOR2.GetValue(1), 2), 2)
                    ''Console.WriteLine("cprice1 = " & decCPrice)
                    Try
                        decCPrice = CDec(readerOR2.GetValue(1))
                    Catch ex As Exception
                    End Try
                Else
                    'decCPrice = FormatNumber(Round(CDbl(readerOR2.GetValue(1)) + (CDbl(readerOR2.GetValue(1)) * (CDbl(readerOR2.GetValue(2)) / 100)), 2), 2)
                    ''Console.WriteLine("cprice2 = " & decCPrice)
                    Try
                        decCPrice = CDec(CDbl(readerOR2.GetValue(1)) + (CDbl(readerOR2.GetValue(1)) * (CDbl(readerOR2.GetValue(2)) / 100)))
                    Catch ex As Exception
                    End Try
                End If
                bAvgPrice = True
            ElseIf readerOR2.GetValue(0).SubString(0, 1) = "R" And decCPrice = 0.0 Then
                If IsDBNull(readerOR2.GetValue(2)) Then
                    'decCPrice = FormatNumber(Round(readerOR2.GetValue(1), 2), 2)
                    ''Console.WriteLine("cprice1 = " & decCPrice)
                    Try
                        decCPrice = CDec(readerOR2.GetValue(1))
                    Catch ex As Exception
                    End Try
                Else
                    'decCPrice = FormatNumber(Round(CDbl(readerOR2.GetValue(1)) + (CDbl(readerOR2.GetValue(1)) * (CDbl(readerOR2.GetValue(2)) / 100)), 2), 2)
                    ''Console.WriteLine("cprice2 = " & decCPrice)
                    Try
                        decCPrice = CDec(CDbl(readerOR2.GetValue(1)) + (CDbl(readerOR2.GetValue(1)) * (CDbl(readerOR2.GetValue(2)) / 100)))
                    Catch ex As Exception
                    End Try
                End If
                bAvgPrice = True
                'jr 29Nov2011 added V
            ElseIf (readerOR2.GetValue(0).SubString(0, 1) = "I") Or _
                    (readerOR2.GetValue(0).SubString(0, 1) = "V") Then
                If IsDBNull(readerOR2.GetValue(2)) Then
                    'decIPrice = FormatNumber(Round(readerOR2.GetValue(1), 2), 2)
                    ''Console.WriteLine("iprice1 = " & decIPrice)
                    Try
                        decIPrice = CDec(readerOR2.GetValue(1))
                    Catch ex As Exception
                    End Try
                Else
                    'decIPrice = FormatNumber(Round(CDbl(readerOR2.GetValue(1)) + (CDbl(readerOR2.GetValue(1)) * (CDbl(readerOR2.GetValue(2)) / 100)), 2), 2)
                    ''Console.WriteLine("iprice2 = " & decIPrice & " " & readerOR2.GetValue(1))
                    Try
                        decIPrice = CDec(CDbl(readerOR2.GetValue(1)) + (CDbl(readerOR2.GetValue(1)) * (CDbl(readerOR2.GetValue(2)) / 100)))
                    Catch ex As Exception
                    End Try
                End If
                bAvgPrice = True
            ElseIf readerOR2.GetValue(0).SubString(0, 1) = "B" And decIPrice = 0.0 Then
                If IsDBNull(readerOR2.GetValue(2)) Then
                    'decIPrice = FormatNumber(Round(readerOR2.GetValue(1), 2), 2)
                    ''Console.WriteLine("iprice1 = " & decIPrice)
                    Try
                        decIPrice = CDec(readerOR2.GetValue(1))
                    Catch ex As Exception
                    End Try
                Else
                    'decIPrice = FormatNumber(Round(CDbl(readerOR2.GetValue(1)) + (CDbl(readerOR2.GetValue(1)) * (CDbl(readerOR2.GetValue(2)) / 100)), 2), 2)
                    ''Console.WriteLine("iprice2 = " & decIPrice)
                    Try
                        decIPrice = CDec(CDbl(readerOR2.GetValue(1)) + (CDbl(readerOR2.GetValue(1)) * (CDbl(readerOR2.GetValue(2)) / 100)))
                    Catch ex As Exception
                    End Try
                End If
                bAvgPrice = True
            End If
        End While
        readerOR2.Close()
        'connectOR.Close()
        If bAvgPrice = True Then

            If decIPrice > 0.0 Then
                Return decIPrice
            Else
                Return decCPrice
            End If
        End If

        Return 0

    End Function

    Function getCPlusdb(ByVal connectCplus As SqlConnection) As Hashtable

        Dim rtn As String = "Module1.getCPlusdb"

        Dim I As Integer
        Dim hshClassids As New Hashtable
        connectCplus.Open()

        Dim commandMSRD As New SqlCommand("SELECT catalogID FROM ClassRoot" & vbCrLf & _
                " WHERE (NOT (catalogID = 0)) AND (publishedFlag = 1)", connectCplus)

        Try
            Dim dataAdapter As New SqlDataAdapter(commandMSRD)
            Dim ds As New DataSet
            dataAdapter.Fill(ds)
            For I = 0 To ds.Tables(0).Rows.Count - 1
                hshClassids.Add(ds.Tables(0).Rows(I).Item("catalogID"), New SqlConnection("server=" & SQLDBPROD & ";uid=sa;pwd=sdiadmin;Initial Catalog=" & ds.Tables(0).Rows(I).Item("catalogID") & ";connection timeout=15;" & ""))
            Next
            connectCplus.Close()

            Return hshClassids

        Catch ex As Exception
            'objStreamWriter.WriteLine("")
            'objStreamWriter.WriteLine("***OLDDB SQL error - " & ex.ToString)
            'objStreamWriter.WriteLine("")
            m_logger.WriteErrorLog(rtn & " :: OLDDB SQL error - " & ex.ToString)
            Trace.Flush()

        End Try

    End Function

    Function getCPlusdbSQL2012(ByVal connectCplus As SqlConnection) As Hashtable

        Dim rtn As String = "Module1.getCPlusdbSQL2012"

        Dim I As Integer
        Dim hshClassids As New Hashtable
        connectCplus.Open()

        Dim commandMSRD As New SqlCommand("SELECT catalogID FROM ClassRoot" & vbCrLf & _
                " WHERE (NOT (catalogID = 0)) AND (publishedFlag = 1)", connectCplus)

        Try
            Dim dataAdapter As New SqlDataAdapter(commandMSRD)
            Dim ds As New DataSet
            dataAdapter.Fill(ds)
            For I = 0 To ds.Tables(0).Rows.Count - 1
                hshClassids.Add(ds.Tables(0).Rows(I).Item("catalogID"), New SqlConnection("server=SQL2012;uid=einternet;pwd=E1nternet$;Initial Catalog=" & ds.Tables(0).Rows(I).Item("catalogID") & ";connection timeout=15;" & ""))
            Next
            connectCplus.Close()

            Return hshClassids

        Catch ex As Exception
            'objStreamWriter.WriteLine("")
            'objStreamWriter.WriteLine("***OLDDB SQL error - " & ex.ToString)
            'objStreamWriter.WriteLine("")
            m_logger.WriteErrorLog(rtn & " :: OLDDB SQL2012 error - " & ex.ToString)
            Trace.Flush()

        End Try

    End Function

    Function getmarkup(ByVal decPrice As String, _
                        ByVal sSite As String, _
                        ByVal sitmtyp As String, _
                        ByVal sstktyp As String, _
                        ByVal connectOR As OleDbConnection) As Decimal
        Dim commandOR As New OleDbCommand("SELECT ISA_MARKUP_RATE" & vbCrLf & _
                        " FROM PS_ISA_PRICE_RULE" & vbCrLf & _
                        " WHERE SHIP_FROM_BU = 'I0" & sSite & "'" & vbCrLf & _
                        " AND ORDER_GRP = '" & sstktyp & "'" & vbCrLf & _
                        " AND INV_ITEM_TYPE = '" & sitmtyp & "'", connectOR)

        Dim readerOR As OleDbDataReader = _
                      commandOR.ExecuteReader

        If readerOR.HasRows() = True Then
            readerOR.Read()
            decPrice = decPrice + (decPrice * Convert.ToDouble(readerOR.Item(0)) / 100)
        Else
            decPrice = 0
        End If
        'While readerOR.Read
        '    decPrice = decPrice + (decPrice * Convert.ToDouble(readerOR.Item(0)) / 100)
        'End While
        readerOR.Close()
        Return decPrice

    End Function

    Private Function ParseCommandParams(ByVal s As String) As Hashtable
        Dim arr As New Hashtable
        s = s.Trim
        Dim sections As String() = s.Split(CChar(" "))
        For Each keyVal As String In sections
            If keyVal.Trim.Length > 0 Then
                Dim s1 As String() = keyVal.Trim.Split(CChar("="))
                ' key
                Dim id As String = ""
                Try
                    id = s1(0).Trim.ToUpper
                Catch ex As Exception
                End Try
                ' value
                Dim val As String = ""
                Try
                    val = s1(1).Trim.ToUpper
                Catch ex As Exception
                End Try
                ' check/add
                If id.Length > 0 Then
                    If arr.ContainsKey(id) Then
                        ' OVERRIDE
                        arr(id) = val
                    Else
                        ' ADD
                        arr.Add(id, val)
                    End If
                End If
            End If
        Next
        Return arr
    End Function

    'Function GetPriceFromSDiExPrice(ByVal strItemID As String) As String
    '    ' Note: This function was moved here from clsShoppingCart.vb and renamed from GetNewPrice.

    '    ' Purpose: Retrieves the price from the new price table for the given item ID.
    '    ' Adds the site prefix to the item ID if needed before executing the query.
    '    ' Return value is formatted to 2 decimal places. If a valid value is not read
    '    ' from the table, defaults to zero.

    '    Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance
    '    Dim strDBPrice As String = ""
    '    Dim ds As DataSet
    '    Dim bGotPrice As Boolean = False

    '    'do lookup using item ID with prefix so make sure item ID has prefix
    '    strItemID = PrependSitePrefix(currentApp.Session("ITEMMODE"), strItemID, currentApp.Session("SITEPREFIX"))

    '    ds = GetDataFromSDiExPrice(strItemID)
    '    Try
    '        If ds.Tables(0).Rows.Count > 0 Then
    '            strDBPrice = ds.Tables(0).Rows(0).Item("PRICE")
    '                If strDBPrice IsNot Nothing Then
    '                If IsNumeric(strDBPrice) Then
    '                    If CType(strDBPrice, Decimal) > 0 Then
    '                        bGotPrice = True
    '                    End If
    '                End If
    '            End If
    '        End If
    '    Catch ex As Exception

    '    End Try

    '    If Not bGotPrice Then
    '        strDBPrice = "0.00"
    '    End If

    '    strDBPrice = String.Format("{0:####,###,##0.00}", CType(strDBPrice, Decimal))

    '    Return strDBPrice

    'End Function

    Function GetSDiExPrice1(ByVal strItemIDs As String, ByVal strBu3 As String) As Decimal
        ' Purpose: Accepts one or more item IDs separated by a comma. Item
        ' IDs are expected to already have the site prefix, if needed, for the 
        ' query. Returns a dataset containing all columns of data for all
        ' the given item IDs.

        If (strItemIDs Is Nothing) Then
            strItemIDs = ""
        End If

        ' trim leading and trailing single quote
        strItemIDs = strItemIDs.Replace("'", "")

        Dim sList As String = ""
        Dim itemList As String() = strItemIDs.Trim.Split(","c)

        For Each s As String In itemList
            If (s.Trim.Length > 0) Then
                sList &= "'" & s.Trim & "',"
            End If
        Next

        sList = sList.TrimEnd(",")
        If (sList.Trim.Length = 0) Then
            ' just so the query below does not grab any unwanted item/s' price/qty and currency if passed value is blank
            sList = "'~'"
        End If

        Dim strSQLstring As String = "" & vbCrLf & _
         "SELECT " & vbCrLf & _
         "  INV_ITEM_ID " & vbCrLf & _
         ", PRICE " & vbCrLf & _
         ", QTY QTY_ONHAND " & vbCrLf & _
         ", CURRENCY_CD " & vbCrLf & _
         "FROM SYSADM.PS_ISA_SDIEX_PRICE " & vbCrLf & _
         "WHERE INV_ITEM_ID IN (" & sList & ")" & vbCrLf & _
         ""
        'If Trim(strBu3) <> "" Then
        '    strBu3 = Trim(strBu3)
        '    If Len(strBu3) = 3 Then
        '        strSQLstring = strSQLstring & " AND ISA_SITE_ID = '" & strBu3 & "'"
        '    End If
        'End If

        Dim strDBPrice As String = " "
        Dim itmPrice As Decimal = CDec("0")
        Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=" & ORDB & "")

        Dim commandOR As New OleDbCommand(strSQLstring, connectOR)
        connectOR.Open()

        Dim dataAdapter As New OleDbDataAdapter(commandOR)
        Dim ds As New DataSet
        Try
            dataAdapter.Fill(ds)
            strDBPrice = ds.Tables(0).Rows(0).Item("PRICE")
            itmPrice = CType(strDBPrice, Decimal)
        Catch ex As Exception
            strDBPrice = "0.00"
            itmPrice = 0
        End Try

        Return itmPrice
    End Function

End Module

