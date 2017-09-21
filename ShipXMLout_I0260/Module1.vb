Imports System.Xml
Imports System.IO
Imports System.Data.OleDb
Imports System.Web.Mail
Imports System
Imports System.Data
Imports System.Web
Imports System.Web.UI
Imports System.Text
Imports System.Net
Imports System.Net.Mail
Imports System.Collections.Generic

Module Module1

    Private m_logger As ApplicationLogger = Nothing

    Private Const oraCN_default_provider As String = "Provider=OraOLEDB.Oracle.1;"
    Private Const oraCN_default_creden As String = "User ID=einternet;Password=einternet;"
    Private Const oraCN_default_DB As String = "Data Source=RPTG;"

    Private Const orderType_FAB As String = "FAB"
    Private Const nstkContainerIdSuffix As String = "NSTK"

    Dim rootDir As String = "C:\SHIPXML\XMLOut260"
    Dim logpath As String = "C:\SHIPXML\XMLOut260\LOGS\UpdSHIPXMLOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

    Private connectOR As OleDbConnection = Nothing

    Private m_oraCNstring As String = "" & _
                                      oraCN_default_provider & _
                                      oraCN_default_creden & _
                                      oraCN_default_DB & _
                                      ""
    Dim objStreamWriter As StreamWriter
    Private m_nstkStartDate As DateTime = Now

    Sub Main()

        Dim rtn As String = "Module1.Main"

        Console.WriteLine("Start Shipping XML out for 260")
        Console.WriteLine("")

        '   (1) connection string / db connection
        Dim cnString As String = ""
        Try
            cnString = My.Settings("oraCNString1").ToString.Trim
        Catch ex As Exception
            cnString = ""
        End Try
        If Trim(cnString) <> "" Then
            m_oraCNstring = cnString
            ' drop current connection and re-create
            Try
                connectOR.Dispose()
            Catch ex As Exception
            End Try
            connectOR = Nothing

            'Try
            '    connectOR = New OleDbConnection(cnString)
            'Catch ex As Exception

            'End Try
        End If

        ' default log level
        Dim logLevel As System.Diagnostics.TraceLevel = TraceLevel.Info

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
            Dim db As String = CStr(param(key:="/DB")).Trim.ToUpper
            m_oraCNstring = "" & _
                            oraCN_default_provider & _
                            oraCN_default_creden & _
                            "Data Source=" & db & ";" & _
                            ""
        End If
        If param.ContainsKey(key:="/NSTK_START_DT") Then
            Try
                m_nstkStartDate = CDate(CStr(param(key:="/NSTK_START_DT")).Trim)
            Catch ex As Exception
            End Try
        End If
        param = Nothing

        ' connection
        connectOR = New OleDbConnection(m_oraCNstring)

        ' initialize log
        m_logger = New ApplicationLogger(logpath, logLevel)

        ' log verbose DB connection string
        m_logger.WriteInformationLog("" & _
                                     System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName & _
                                     " ; Version: " & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & _
                                     "")
        m_logger.WriteInformationLog(rtn & " :: Start of SP Processing XML out I0260 version.")
        m_logger.WriteVerboseLog(rtn & " :: connection string : [" & connectOR.ConnectionString & "]")

        m_logger.WriteInformationLog(rtn & " :: Update of SP Processing XML out I0260 version.")

        Dim bolError As Boolean = buildPickingXMLOut()

        If bolError Then

            m_logger.WriteInformationLog(rtn & " :: Sending Error email for I0260 version.")

            SendEmail(bolError)
        End If

        ' final log for this run
        m_logger.WriteInformationLog(rtn & " :: Ending XML out I0260 version process.")

        ' destroy logger object
        Try
            m_logger.Dispose()
        Catch ex As Exception
        Finally
            m_logger = Nothing
        End Try
    End Sub

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

#Region " (old) buildPickingXMLOut() function "
    ''Private Function buildPickingXMLOut() As Boolean

    ''    Dim rtn As String = "Module1.buildPickingXMLOut"

    ''    Dim ds As New DataSet
    ''    Dim bolerror As Boolean = False
    ''    Dim strSQLstring As String = "" & _
    ''                    "SELECT A.SHIP_CNTR_ID, B.ORDER_NO " & vbCrLf & _
    ''                    "FROM " & vbCrLf & _
    ''                    " PS_ISA_PICKING_CNT A " & vbCrLf & _
    ''                    ",PS_ISA_PICKING_INT B " & vbCrLf & _
    ''                    "WHERE A.BUSINESS_UNIT = B.BUSINESS_UNIT " & vbCrLf & _
    ''                    "  AND A.DEMAND_SOURCE = B.DEMAND_SOURCE " & vbCrLf & _
    ''                    "  AND A.SOURCE_BUS_UNIT = B.SOURCE_BUS_UNIT " & vbCrLf & _
    ''                    "  AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
    ''                    "  AND A.ORDER_INT_LINE_NO = B.ORDER_INT_LINE_NO " & vbCrLf & _
    ''                    "  AND A.SCHED_LINE_NO = B.SCHED_LINE_NO " & vbCrLf & _
    ''                    "  AND A.INV_ITEM_ID = B.INV_ITEM_ID " & vbCrLf & _
    ''                    "  AND A.DEMAND_LINE_NO = B.DEMAND_LINE_NO " & vbCrLf & _
    ''                    "  AND A.SEQ_NBR = B.SEQ_NBR " & vbCrLf & _
    ''                    "  AND A.RECEIVER_ID = B.RECEIVER_ID " & vbCrLf & _
    ''                    "  AND A.RECV_LN_NBR = B.RECV_LN_NBR " & vbCrLf & _
    ''                    "  AND A.DEMAND_SOURCE = 'OM' " & vbCrLf & _
    ''                    "  AND A.SOURCE_BUS_UNIT LIKE '%260' " & vbCrLf & _
    ''                    "  AND B.SHIP_DTTM IS NOT NULL " & vbCrLf & _
    ''                    "  AND B.SHIP_DTTM > TO_DATE(SUBSTR('2009-12-31-23.59.00.000000', 0, 19),'YYYY-MM-DD-HH24.MI.SS') " & vbCrLf & _
    ''                    "  AND NOT EXISTS ( " & vbCrLf & _
    ''                    "                   SELECT C.SHIP_CNTR_ID " & vbCrLf & _
    ''                    "                   FROM PS_ISA_PICKING_SHP C " & vbCrLf & _
    ''                    "                   WHERE C.SHIP_CNTR_ID = A.SHIP_CNTR_ID " & vbCrLf & _
    ''                    "                 ) " & vbCrLf & _
    ''                    "GROUP BY A.SHIP_CNTR_ID, B.ORDER_NO " & vbCrLf & _
    ''                    ""

    ''    m_logger.WriteVerboseLog(rtn & " :: executing : " & vbCrLf & strSQLstring)
    ''    Try
    ''        ds = ORDBAccess.GetAdapter(strSQLstring, connectOR)
    ''        m_logger.WriteVerboseLog(rtn & " :: executed with " & ds.Tables(0).Rows.Count.ToString & " record(s) returned.")
    ''    Catch OleDBExp As OleDbException
    ''        Console.WriteLine("")
    ''        Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
    ''        Console.WriteLine("")
    ''        connectOR.Close()
    ''        m_logger.WriteErrorLog(rtn & " :: Error - error reading transaction FROM PS_ISA_PICKING_CNT A, PS_ISA_PICKING_INT B")
    ''        Return True
    ''    End Try

    ''    If ds.Tables(0).Rows.Count = 0 Then
    ''        m_logger.WriteWarningLog(rtn & " :: Warning - no containers to process at this time.")
    ''        Return True
    ''    End If

    ''    Dim I As Integer
    ''    Dim strContainerID As String
    ''    Dim rowsaffected As Integer
    ''    Dim dteJulian As Integer
    ''    Dim dteStart As Date = "01/01/1900"

    ''    dteJulian = DateDiff(DateInterval.Day, dteStart, Now())

    ''    Dim runDT As DateTime = Now
    ''    ''Dim strXMLPath As String = rootDir & "\XMLOUT\DOE" & Convert.ToString(dteJulian) & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & ".xml"
    ''    'Dim strXMLPath As String = rootDir & "\XMLOUT\DOE" & runDT.ToString(Format:="MMddyy_HHmmss") & ".XML"
    ''    Dim strXMLPath As String = rootDir & "\XMLOUT\DOE" & runDT.ToString(Format:="MMddyy_HHmmss") & ".txt"
    ''    'Dim objXMLWriter As XmlTextWriter = Nothing
    ''    Dim objXMLWriter As StreamWriter = Nothing

    ''    Try
    ''        'objXMLWriter = New XmlTextWriter(strXMLPath, System.Text.Encoding.UTF8)
    ''        objXMLWriter = New StreamWriter(Path:=strXMLPath, append:=True)
    ''        m_logger.WriteInformationLog(rtn & " :: Writing to file: " & strXMLPath)
    ''    Catch objError As Exception
    ''        m_logger.WriteErrorLog(rtn & " :: Error while accessing document " & strXMLPath & " -> [" & objError.Message & "]")
    ''        Return True
    ''    End Try

    ''    ' consolidate container info per MR
    ''    Dim arr As New ArrayList
    ''    If ds.Tables(0).Rows.Count > 0 Then
    ''        Dim currOrder As orderShipmentInfo = Nothing
    ''        Dim currCntr As containerInfo = Nothing
    ''        Dim orderNo As String = ""
    ''        For Each row As DataRow In ds.Tables(0).Rows
    ''            ' retrieve container Id
    ''            strContainerID = ""
    ''            Try
    ''                strContainerID = CStr(row.Item("SHIP_CNTR_ID"))
    ''                m_logger.WriteVerboseLog(rtn & " :: processing " & strContainerID & " information.")
    ''            Catch ex As Exception
    ''            End Try

    ''            ' retrieve order number
    ''            orderNo = ""
    ''            Try
    ''                orderNo = CStr(row.Item("ORDER_NO"))
    ''            Catch ex As Exception
    ''            End Try

    ''            ' get order/shipping info
    ''            Dim objShippingCnt As New clsShippingData(strContainerID, connectOR, orderNo)
    ''            If Trim(objShippingCnt.OrderNo) = "" Then
    ''                m_logger.WriteErrorLog(rtn & " :: Error - container record not found for - " & strContainerID)
    ''                bolerror = True
    ''            End If

    ''            currOrder = New orderShipmentInfo
    ''            currOrder.OrderNo = orderNo
    ''            currOrder.Shipper = "MoveWay"

    ''            currCntr = New containerInfo(currOrder, strContainerID, objShippingCnt.WorkOrderNo)
    ''            currCntr.ShipDateTime = objShippingCnt.PackDTTM

    ''            currOrder.Containers.Add(currCntr)

    ''            ' check/add
    ''            'if not (objshippingcnt is nothing
    ''            If currOrder.OrderNo.Length > 0 And _
    ''               currCntr.ContainerId.Length > 0 And _
    ''               currCntr.ShipDateTime.Length > 0 And _
    ''               IsDate(currCntr.ShipDateTime) Then
    ''                Dim bIsFound As Boolean = False
    ''                For Each o As orderShipmentInfo In arr
    ''                    If o.Id = currOrder.Id Then
    ''                        Dim cntr As containerInfo = o.GetContainer(currCntr)
    ''                        If (cntr Is Nothing) Then
    ''                            ' add the container
    ''                            currCntr.Parent = o
    ''                            o.Containers.Add(currCntr)
    ''                        Else
    ''                            ' update container info
    ''                            If cntr.ShipDateTime.Length = 0 Then
    ''                                cntr.ShipDateTime = currCntr.ShipDateTime
    ''                                cntr.WorkOrderNo = currCntr.WorkOrderNo
    ''                            Else
    ''                                If CDate(cntr.ShipDateTime) < CDate(currCntr.ShipDateTime) Then
    ''                                    cntr.ShipDateTime = currCntr.ShipDateTime
    ''                                    cntr.WorkOrderNo = currCntr.WorkOrderNo
    ''                                End If
    ''                            End If
    ''                        End If
    ''                        bIsFound = True
    ''                        Exit For
    ''                    End If
    ''                Next
    ''                If Not bIsFound Then
    ''                    arr.Add(currOrder)
    ''                End If
    ''            End If
    ''        Next
    ''    End If




    ''    'objXMLWriter.Formatting = Formatting.Indented
    ''    'objXMLWriter.Indentation = 3
    ''    'objXMLWriter.WriteStartDocument(standalone:=True)
    ''    ''objXMLWriter.WriteStartElement("diffgr:diffgram xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"" xmlns:diffgr=""urn:schemas-microsoft-com:xml-diffgram-v1""")
    ''    'objXMLWriter.WriteStartElement("diffgr:diffgram")
    ''    'objXMLWriter.WriteAttributeString(localName:="xmlns:msdata", value:="urn:schemas-microsoft-com:xml-msdata")
    ''    'objXMLWriter.WriteAttributeString(localName:="xmlns:diffgr", value:="urn:schemas-microsoft-com:xml-diffgram-v1")
    ''    ''objXMLWriter.WriteComment("Created on " & Now())
    ''    'objXMLWriter.WriteStartElement("NewDataSet")

    ''    'objXMLWriter.Flush()

    ''    Dim flushCtr As Integer = 0

    ''    If arr.Count > 0 Then
    ''        Dim currOrder As orderShipmentInfo = Nothing
    ''        'For I = 0 To ds.Tables(0).Rows.Count - 1
    ''        For I = 0 To arr.Count - 1
    ''            currorder = CType(arr(I), orderShipmentInfo)

    ''            ''strContainerID = ds.Tables(0).Rows(I).Item("SHIP_CNTR_ID")
    ''            ''m_logger.WriteVerboseLog(rtn & " :: processing " & strContainerID & " information.")

    ''            ''Dim objShippingCnt As New clsShippingData(strContainerID, connectOR)
    ''            ''If Trim(objShippingCnt.OrderNo) = "" Then
    ''            ''    m_logger.WriteErrorLog(rtn & " :: Error - container record not found for " & strContainerID)
    ''            ''    bolerror = True
    ''            ''End If

    ''            'objXMLWriter.WriteStartElement("JOB")
    ''            'objXMLWriter.WriteAttributeString(localName:="diffgr:id", value:="JOB" & (I + 1).ToString)
    ''            'objXMLWriter.WriteAttributeString(localName:="msdata:rowOrder", value:=(I + 1).ToString)

    ''            Dim dte As String = ""
    ''            Dim tme As String = ""
    ''            'If objShippingCnt.PackDTTM.Trim.Length > 0 Then
    ''            '    Try
    ''            '        dte = CDate(objShippingCnt.PackDTTM.Trim).ToString("yyyy-MM-dd")
    ''            '    Catch ex As Exception
    ''            '    End Try
    ''            '    Try
    ''            '        tme = CDate(objShippingCnt.PackDTTM.Trim).ToString("HH:mm:ss")
    ''            '    Catch ex As Exception
    ''            '    End Try
    ''            'End If
    ''            If currorder.ShipDateTime.Length > 0 Then
    ''                Try
    ''                    dte = CDate(currorder.ShipDateTime.Trim).ToString("yyyy-MM-dd")
    ''                Catch ex As Exception
    ''                End Try
    ''                Try
    ''                    tme = CDate(currorder.ShipDateTime.Trim).ToString("HH:mm:ss")
    ''                Catch ex As Exception
    ''                End Try
    ''            End If

    ''            Dim workOrderNos As String = ""
    ''            If currorder.Containers.Count > 0 Then
    ''                For Each c As containerInfo In currorder.Containers
    ''                    If workOrderNos.IndexOf(c.WorkOrderNo) = -1 Then
    ''                        workOrderNos &= c.WorkOrderNo.Trim & ","
    ''                        ' just get the first one
    ''                        Exit For
    ''                    End If
    ''                Next
    ''                If workOrderNos.Length > 0 Then
    ''                    workOrderNos = workOrderNos.TrimEnd(","c)
    ''                End If
    ''            End If

    ''            'objXMLWriter.WriteElementString("BUDGET", currorder.OrderNo)
    ''            ''objXMLWriter.WriteElementString("TICKETNO", strContainerID)
    ''            'objXMLWriter.WriteElementString("TICKETNO", currorder.Containers.Count.ToString)
    ''            'objXMLWriter.WriteElementString("WORKORDER", workOrderNos)
    ''            'objXMLWriter.WriteElementString("CDATE", dte)
    ''            'objXMLWriter.WriteElementString("CTIME", tme)
    ''            'objXMLWriter.WriteElementString("POD", currorder.Shipper)

    ''            'objXMLWriter.WriteEndElement()

    ''            Dim xOrderNo As String = (Space(10) & currorder.OrderNo.Trim)
    ''            xOrderNo = xOrderNo.Substring(xOrderNo.Length - 10, 10)
    ''            Dim xContainerId As String = (Space(3) & currorder.Containers.Count.ToString(Format:="000"))
    ''            xContainerId = xContainerId.Substring(xContainerId.Length - 3, 3)
    ''            Dim xWO As String = (Space(20) & workOrderNos)
    ''            xWO = xWO.Substring(xWO.Length - 20, 20)
    ''            Dim xDte As String = (Space(10) & dte)
    ''            xDte = xDte.Substring(xDte.Length - 10, 10)
    ''            Dim xTme As String = (Space(8) & tme)
    ''            xTme = xTme.Substring(xTme.Length - 8, 8)
    ''            Dim xShipr As String = (Space(10) & currorder.Shipper)
    ''            xShipr = xShipr.Substring(xShipr.Length - 10, 10)

    ''            Dim s3 As String = "" & _
    ''                               xOrderNo & _
    ''                               xContainerId & _
    ''                               xWO & _
    ''                               xDte & _
    ''                               xTme & _
    ''                               xShipr & _
    ''                               ""
    ''            objXMLWriter.WriteLine(s3)

    ''            If currorder.Containers.Count > 0 Then
    ''                For Each c As containerInfo In currorder.Containers
    ''                    strSQLstring = "INSERT INTO PS_ISA_PICKING_SHP" & vbCrLf & _
    ''                                " (SHIP_CNTR_ID," & vbCrLf & _
    ''                                " ISA_XML_DATE," & vbCrLf & _
    ''                                " ISA_XML_TIME," & vbCrLf & _
    ''                                " ISA_TICKETNO," & vbCrLf & _
    ''                                " ISA_BUDGET," & vbCrLf & _
    ''                                " ISA_WORKORDER," & vbCrLf & _
    ''                                " ISA_CDATE," & vbCrLf & _
    ''                                " ISA_CTIME," & vbCrLf & _
    ''                                " ISA_POD," & vbCrLf & _
    ''                                " LASTUPDDTTM)" & vbCrLf & _
    ''                                " VALUES " & vbCrLf & _
    ''                                " ('" & c.ContainerId & "'," & vbCrLf & _
    ''                                " TO_DATE('" & Now.Date & "', 'MM/DD/YYYY')," & vbCrLf & _
    ''                                " '" & Now.ToLongTimeString & "'," & vbCrLf & _
    ''                                " ' '," & vbCrLf & _
    ''                                " ' '," & vbCrLf & _
    ''                                " ' '," & vbCrLf & _
    ''                                " ''," & vbCrLf & _
    ''                                " ' '," & vbCrLf & _
    ''                                " ' '," & vbCrLf & _
    ''                                " TO_DATE('" & Now & "', 'MM/DD/YYYY HH:MI:SS AM'))" & _
    ''                                ""
    ''                    Try
    ''                        Dim Command = New OleDbCommand(strSQLstring, connectOR)
    ''                        m_logger.WriteVerboseLog(rtn & " :: command object created.")
    ''                        connectOR.Open()
    ''                        m_logger.WriteVerboseLog(rtn & " :: connection opened.")
    ''                        m_logger.WriteVerboseLog(rtn & " :: executing : " & strSQLstring)
    ''                        rowsaffected = Command.ExecuteNonQuery()
    ''                        m_logger.WriteVerboseLog(rtn & " :: executed with " & rowsaffected.ToString & " record(s) affected.")
    ''                        connectOR.Close()
    ''                        m_logger.WriteVerboseLog(rtn & " :: connection closed.")
    ''                    Catch OleDBExp As OleDbException
    ''                        Console.WriteLine("")
    ''                        Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
    ''                        Console.WriteLine("")
    ''                        connectOR.Close()
    ''                        'objStreamWriter.WriteLine("  Error - Insert error for container " & strContainerID & " " & OleDBExp.ToString)
    ''                        m_logger.WriteErrorLog(rtn & " :: Error - Insert error for container " & strContainerID & " " & OleDBExp.ToString)
    ''                        bolerror = True
    ''                    End Try
    ''                Next
    ''            End If

    ''            ' hard write every 5 records
    ''            If flushCtr > 4 Then
    ''                objXMLWriter.Flush()
    ''            Else
    ''                flushCtr += 1
    ''            End If
    ''        Next
    ''    End If

    ''    'objXMLWriter.WriteEndElement()
    ''    'objXMLWriter.WriteEndElement()
    ''    objXMLWriter.Flush()
    ''    objXMLWriter.Close()

    ''    ' why???
    ''    Dim strXMLResult As String
    ''    Dim objSR As StreamReader = File.OpenText(strXMLPath)
    ''    strXMLResult = objSR.ReadToEnd()
    ''    objSR.Close()
    ''    objSR = Nothing

    ''    If bolerror = True Then
    ''        Return True
    ''    Else
    ''        Return False
    ''    End If
    ''End Function
#End Region

    Private Function buildPickingXMLOut() As Boolean

        Dim rtn As String = "Module1.buildPickingXMLOut"

        'Dim ds As New DataSet
        Dim bolerror As Boolean = False

        Dim sql As String = "" & _
"SELECT PICK.* " & vbCrLf & _
"FROM " & vbCrLf & _
"( " & vbCrLf & _
"SELECT " & vbCrLf & _
" A.BUSINESS_UNIT " & vbCrLf & _
",A.DEMAND_SOURCE " & vbCrLf & _
",A.SOURCE_BUS_UNIT " & vbCrLf & _
",A.ORDER_NO " & vbCrLf & _
",A.ORDER_INT_LINE_NO " & vbCrLf & _
",SUM(A.QTY_PICKED) AS QTY_PICKED " & vbCrLf & _
",MAX(A.SHIP_DTTM) AS SHIP_DTTM " & vbCrLf & _
",(" & vbCrLf & _
"  SELECT B11.ISA_WORK_ORDER_NO" & vbCrLf & _
"  FROM PS_ISA_ORD_INTFC_H A11 " & vbCrLf & _
"  ,PS_ISA_ORD_INTFC_L B11 " & vbCrLf & _
"  ,PS_ISA_ORD_INTFC_O C11 " & vbCrLf & _
"  WHERE A11.ISA_IDENTIFIER = B11.ISA_PARENT_IDENT " & vbCrLf & _
"    AND A11.ORDER_NO = C11.ORDER_NO " & vbCrLf & _
"    AND B11.LINE_NBR = C11.INTFC_LINE_NUM " & vbCrLf & _
"    AND A11.ORDER_NO = A.ORDER_NO " & vbCrLf & _
"    AND C11.ORDER_INT_LINE_NO = A.ORDER_INT_LINE_NO " & vbCrLf & _
"    AND B11.LINE_NBR < 100 " & vbCrLf & _
"    AND B11.ISA_WORK_ORDER_NO <> ' ' " & vbCrLf & _
"    AND ROWNUM < 2 " & vbCrLf & _
"  GROUP BY B11.ISA_WORK_ORDER_NO " & vbCrLf & _
" ) AS ISA_WORK_ORDER_NO " & vbCrLf & _
",(" & vbCrLf & _
"  SELECT B22.ISA_MACHINE_NO " & vbCrLf & _
"  FROM PS_ISA_ORD_INTFC_H A22 " & vbCrLf & _
"  ,PS_ISA_ORD_INTFC_L B22 " & vbCrLf & _
"  ,PS_ISA_ORD_INTFC_O C22 " & vbCrLf & _
"  WHERE A22.ISA_IDENTIFIER = B22.ISA_PARENT_IDENT " & vbCrLf & _
"    AND A22.ORDER_NO = C22.ORDER_NO " & vbCrLf & _
"    AND B22.LINE_NBR = C22.INTFC_LINE_NUM " & vbCrLf & _
"    AND A22.ORDER_NO = A.ORDER_NO " & vbCrLf & _
"    AND C22.ORDER_INT_LINE_NO = A.ORDER_INT_LINE_NO " & vbCrLf & _
"    AND B22.LINE_NBR < 100 " & vbCrLf & _
"    AND ROWNUM < 2 " & vbCrLf & _
"  GROUP BY B22.ISA_MACHINE_NO " & vbCrLf & _
" ) AS ISA_MACHINE_NO " & vbCrLf & _
"FROM PS_ISA_PICKING_INT A " & vbCrLf & _
",( " & vbCrLf & _
"  select " & vbCrLf & _
"   PICK1.DEMAND_SOURCE  " & vbCrLf & _
"  ,PICK1.SOURCE_BUS_UNIT  " & vbCrLf & _
"  ,PICK1.ORDER_NO  " & vbCrLf & _
"  from  " & vbCrLf & _
"   PS_ISA_PICKING_INT PICK1  " & vbCrLf & _
"  ,PS_ISA_PICKING_CNT CNTR1  " & vbCrLf & _
"  ,PS_ISA_PICKING_SHP SHIP1  " & vbCrLf & _
"  where pick1.SOURCE_BUS_UNIT like '%260' " & vbCrLf & _
"    and pick1.demand_source = 'OM' " & vbCrLf & _
"    and pick1.enddttm is not null " & vbCrLf & _
"    and pick1.ship_dttm is not null " & vbCrLf & _
"    and (pick1.BEGINDTTM between trunc(sysdate-120) and trunc(sysdate)) " & vbCrLf & _
"    AND PICK1.BUSINESS_UNIT = CNTR1.BUSINESS_UNIT (+) " & vbCrLf & _
"    AND PICK1.DEMAND_SOURCE = CNTR1.DEMAND_SOURCE (+) " & vbCrLf & _
"    AND PICK1.SOURCE_BUS_UNIT = CNTR1.SOURCE_BUS_UNIT (+) " & vbCrLf & _
"    AND PICK1.ORDER_NO = CNTR1.ORDER_NO (+) " & vbCrLf & _
"    AND PICK1.ORDER_INT_LINE_NO = CNTR1.ORDER_INT_LINE_NO (+) " & vbCrLf & _
"    AND PICK1.SCHED_LINE_NO = CNTR1.SCHED_LINE_NO (+) " & vbCrLf & _
"    AND PICK1.INV_ITEM_ID = CNTR1.INV_ITEM_ID (+) " & vbCrLf & _
"    AND PICK1.DEMAND_LINE_NO = CNTR1.DEMAND_LINE_NO (+) " & vbCrLf & _
"    AND PICK1.SEQ_NBR = CNTR1.SEQ_NBR (+) " & vbCrLf & _
"    AND CNTR1.SHIP_CNTR_ID = SHIP1.SHIP_CNTR_ID (+) " & vbCrLf & _
"    AND SHIP1.SHIP_CNTR_ID IS NULL " & vbCrLf & _
"  group by  " & vbCrLf & _
"   PICK1.DEMAND_SOURCE  " & vbCrLf & _
"  ,PICK1.SOURCE_BUS_UNIT  " & vbCrLf & _
"  ,PICK1.ORDER_NO  " & vbCrLf & _
" ) B " & vbCrLf & _
"WHERE NOT EXISTS " & vbCrLf & _
"            (  SELECT 'Z' " & vbCrLf & _
"               FROM  ( " & vbCrLf & _
"                       SELECT DISTINCT A.ORDER_NO" & vbCrLf & _
"                       FROM PS_ISA_PICKING_INT A" & vbCrLf & _
"                       WHERE A.DEMAND_SOURCE = 'OM'" & vbCrLf & _
"                         AND A.SOURCE_BUS_UNIT like '%260'" & vbCrLf & _
"                         AND (A.QTY_PICKED = 0 OR A.SHIP_DTTM IS NULL)" & vbCrLf & _
"                     ) W1 " & vbCrLf & _
"               WHERE  B.ORDER_NO = W1.ORDER_NO" & vbCrLf & _
"            )" & vbCrLf & _
"  AND A.DEMAND_SOURCE = B.DEMAND_SOURCE " & vbCrLf & _
"  AND A.SOURCE_BUS_UNIT = B.SOURCE_BUS_UNIT " & vbCrLf & _
"  AND A.ORDER_NO = B.ORDER_NO " & vbCrLf & _
"GROUP BY A.BUSINESS_UNIT, A.DEMAND_SOURCE, A.SOURCE_BUS_UNIT, A.ORDER_NO, A.ORDER_INT_LINE_NO " & vbCrLf & _
") PICK " & vbCrLf & _
"ORDER BY PICK.DEMAND_SOURCE, PICK.SOURCE_BUS_UNIT, PICK.ORDER_NO, PICK.ORDER_INT_LINE_NO " & vbCrLf & _
                            ""

        Dim rdr As OleDbDataReader = Nothing
        m_logger.WriteVerboseLog(rtn & " :: executing : " & vbCrLf & sql)
        Try
            rdr = ORDBAccess.GetReader(sql, connectOR)
            'm_logger.WriteVerboseLog(rtn & " :: executed with " & ds.Tables(0).Rows.Count.ToString & " record(s) returned.")
        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            connectOR.Close()
            m_logger.WriteErrorLog(rtn & " :: Error - error reading transaction FROM PS_ISA_PICKING_CNT A, PS_ISA_PICKING_INT B for I0260 version.")
            Return True
        End Try

        Dim arr As New processBatch

        If Not (rdr Is Nothing) Then
            Dim currOrder As orderShipmentInfo = Nothing
            Dim currOrderLine As orderLineInfo = Nothing
            Dim demandSrc As String = ""
            Dim orderNo As String = ""
            Dim orderLn As Integer = -1
            Dim workOrderNo As String = ""
            Dim machineNo As String = ""
            Dim qty As Decimal = 0
            Dim shipDT As String = ""

            While rdr.Read

                orderNo = ""
                Try
                    orderNo = CStr(rdr("ORDER_NO"))
                Catch ex As Exception
                End Try

                orderLn = -1
                Try
                    orderLn = CInt(rdr("ORDER_INT_LINE_NO"))
                Catch ex As Exception
                End Try

                qty = 0
                Try
                    qty = CDec(rdr("QTY_PICKED"))
                Catch ex As Exception
                End Try

                shipDT = ""
                Try
                    shipDT = CDate(rdr("SHIP_DTTM")).ToString
                Catch ex As Exception
                End Try

                demandSrc = ""
                Try
                    demandSrc = CStr(rdr("DEMAND_SOURCE")).Trim
                Catch ex As Exception
                End Try

                workOrderNo = ""
                Try
                    workOrderNo = CStr(rdr("ISA_WORK_ORDER_NO")).Trim
                Catch ex As Exception
                End Try

                machineNo = ""
                Try
                    machineNo = CStr(rdr("ISA_MACHINE_NO")).Trim.ToUpper
                Catch ex As Exception
                End Try

                currOrder = Nothing

                If arr.OrderNoList.IndexOf(orderNo) > -1 Then
                    ' order already added
                    currOrder = arr.GetOrder(orderNo)
                    currOrder.DemandSource = demandSrc
                Else
                    ' order not found in collection yet
                    currOrder = New orderShipmentInfo
                    currOrder.DemandSource = demandSrc
                    currOrder.OrderNo = orderNo
                    currOrder.Shipper = "MoveWay"
                    currOrder.IsFABOrder = (machineNo = orderType_FAB)
                    arr.Orders.Add(currOrder)
                End If

                If Not (currOrder Is Nothing) Then
                    currOrderLine = New orderLineInfo
                    currOrderLine.Parent = currOrder
                    currOrderLine.LineNo = orderLn
                    currOrderLine.QuantityPicked = qty
                    currOrderLine.ShipDateTime = shipDT
                    currOrderLine.WorkOrderNo = workOrderNo
                    If Not currOrder.IsFABOrder And _
                       (machineNo = orderType_FAB) Then
                        currOrder.IsFABOrder = (machineNo = orderType_FAB)
                    End If
                    currOrder.OrderLines.Add(currOrderLine)
                Else
                    ' what to do ???
                End If  'If not (currorder Is Nothing ) Then

            End While
        End If

        If Not (arr.Orders.Count > 0) Then
            m_logger.WriteWarningLog(rtn & " :: Warning - no containers (stock order) to process at this time for I0260 version.")
            'Return True
        End If

        Try
            connectOR.Close()
        Catch ex As Exception
        End Try

        If (arr.Orders.Count > 0) Then
            Dim cntrs As ArrayList = Nothing
            For Each o As orderShipmentInfo In arr.Orders
                cntrs = retrieveContainerIds(o.DemandSource, o.OrderNo)
                If cntrs.Count > 0 Then
                    For Each c As containerInfo In cntrs
                        c.Parent = o
                        o.Containers.Add(c)
                    Next
                End If
            Next
        End If




        '// retrieve and add any non-stock order if exists for this run
        Dim nstkBatch As processBatch = processNSTKOrders()

        If Not (nstkBatch Is Nothing) Then
            If nstkBatch.Orders.Count > 0 Then
                For Each o As orderShipmentInfo In nstkBatch.Orders
                    If Not (o Is Nothing) Then
                        If o.IsOrderFullyShipped Then
                            Dim c As New containerInfo
                            c.Parent = o
                            c.ContainerId = o.OrderNo & nstkContainerIdSuffix
                            o.Containers.Add(c)
                            arr.Orders.Add(o)
                            c = Nothing
                            m_logger.WriteVerboseLog(rtn & " :: order " & o.OrderNo & " included for further processing for I0260 version.")
                        Else
                            m_logger.WriteVerboseLog(rtn & " :: order " & o.OrderNo & " was not fully shipped. Excluding order for I0260 version.")
                        End If
                    End If
                Next
            End If
        End If

        If Not (arr.Orders.Count > 0) Then
            m_logger.WriteWarningLog(rtn & " :: Warning - no containers (stock nor non-stock order) to process at this time for I0260 version.")
            Return False
        End If





        Dim I As Integer
        Dim rowsaffected As Integer
        Dim dteJulian As Integer
        Dim dteStart As Date = "01/01/1900"

        dteJulian = DateDiff(DateInterval.Day, dteStart, Now())

        Dim runDT As DateTime = Now
        Dim strXMLPath As String = rootDir & "\XMLOUT\DOE" & runDT.ToString(format:="MMddyy_HHmmss") & ".txt"
        Dim objXMLWriter As StreamWriter = Nothing

        Try
            objXMLWriter = New StreamWriter(Path:=strXMLPath, append:=True)
            m_logger.WriteInformationLog(rtn & " :: Writing to file: " & strXMLPath)
        Catch objError As Exception
            m_logger.WriteErrorLog(rtn & " :: Error while accessing document " & strXMLPath & " -> [" & objError.Message & "]")
            Return True
        End Try

        Dim flushCtr As Integer = 0

        If arr.Orders.Count > 0 Then
            Dim currOrder As orderShipmentInfo = Nothing

            For I = 0 To arr.Orders.Count - 1
                currOrder = CType(arr.Orders(I), orderShipmentInfo)
                ' according to Scott, do not include FAB orders
                '   - erwin 2010.05.24
                If Not currOrder.IsFABOrder Then
                    Dim dte As String = ""
                    Dim tme As String = ""

                    If currOrder.ShipDateTime.Length > 0 Then
                        Try
                            dte = CDate(currOrder.ShipDateTime.Trim).ToString("yyyy-MM-dd")
                        Catch ex As Exception
                        End Try
                        Try
                            tme = CDate(currOrder.ShipDateTime.Trim).ToString("HH:mm:ss")
                        Catch ex As Exception
                        End Try
                    End If

                    Dim workOrderNos As String = ""

                    If currOrder.OrderLines.Count > 0 Then
                        For Each c As orderLineInfo In currOrder.OrderLines
                            If workOrderNos.IndexOf(c.WorkOrderNo) = -1 Then
                                workOrderNos &= c.WorkOrderNo.Trim & ","
                                ' just get the first one
                                Exit For
                            End If
                        Next
                        If workOrderNos.Length > 0 Then
                            workOrderNos = workOrderNos.TrimEnd(","c)
                        End If
                    End If

                    Dim xOrderNo As String = (Space(10) & currOrder.OrderNo.Trim)
                    xOrderNo = xOrderNo.Substring(xOrderNo.Length - 10, 10)
                    Dim xContainerId As String = (Space(3) & currOrder.Containers.Count.ToString(format:="000"))
                    xContainerId = xContainerId.Substring(xContainerId.Length - 3, 3)
                    Dim xWO As String = (Space(20) & workOrderNos)
                    xWO = xWO.Substring(xWO.Length - 20, 20)
                    Dim xDte As String = (Space(10) & dte)
                    xDte = xDte.Substring(xDte.Length - 10, 10)
                    Dim xTme As String = (Space(8) & tme)
                    xTme = xTme.Substring(xTme.Length - 8, 8)
                    Dim xShipr As String = (Space(10) & currOrder.Shipper)
                    xShipr = xShipr.Substring(xShipr.Length - 10, 10)

                    Dim s3 As String = "" & _
                                       xOrderNo & _
                                       xContainerId & _
                                       xWO & _
                                       xDte & _
                                       xTme & _
                                       xShipr & _
                                       ""
                    objXMLWriter.WriteLine(s3)

                    If currOrder.Containers.Count > 0 Then
                        Dim strSQLstring As String = ""
                        For Each c As containerInfo In currOrder.Containers
                            strSQLstring = "INSERT INTO PS_ISA_PICKING_SHP" & vbCrLf & _
                                        " (SHIP_CNTR_ID," & vbCrLf & _
                                        " ISA_XML_DATE," & vbCrLf & _
                                        " ISA_XML_TIME," & vbCrLf & _
                                        " ISA_TICKETNO," & vbCrLf & _
                                        " ISA_BUDGET," & vbCrLf & _
                                        " ISA_WORKORDER," & vbCrLf & _
                                        " ISA_CDATE," & vbCrLf & _
                                        " ISA_CTIME," & vbCrLf & _
                                        " ISA_POD," & vbCrLf & _
                                        " LASTUPDDTTM)" & vbCrLf & _
                                        " VALUES " & vbCrLf & _
                                        " ('" & c.ContainerId & "'," & vbCrLf & _
                                        " TO_DATE('" & Now.Date & "', 'MM/DD/YYYY')," & vbCrLf & _
                                        " '" & Now.ToLongTimeString & "'," & vbCrLf & _
                                        " ' '," & vbCrLf & _
                                        " ' '," & vbCrLf & _
                                        " ' '," & vbCrLf & _
                                        " ''," & vbCrLf & _
                                        " ' '," & vbCrLf & _
                                        " ' '," & vbCrLf & _
                                        " TO_DATE('" & Now & "', 'MM/DD/YYYY HH:MI:SS AM'))" & _
                                        ""
                            Try
                                Dim Command = New OleDbCommand(strSQLstring, connectOR)
                                m_logger.WriteVerboseLog(rtn & " :: command object created.")
                                connectOR.Open()
                                m_logger.WriteVerboseLog(rtn & " :: connection opened.")
                                m_logger.WriteVerboseLog(rtn & " :: executing : " & strSQLstring)
                                rowsaffected = Command.ExecuteNonQuery()
                                m_logger.WriteVerboseLog(rtn & " :: executed with " & rowsaffected.ToString & " record(s) affected.")
                                connectOR.Close()
                                m_logger.WriteVerboseLog(rtn & " :: connection closed.")
                            Catch OleDBExp As OleDbException
                                Console.WriteLine("")
                                Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                                Console.WriteLine("")
                                connectOR.Close()
                                'objStreamWriter.WriteLine("  Error - Insert error for container " & strContainerID & " " & OleDBExp.ToString)
                                m_logger.WriteErrorLog(rtn & " :: Error - Insert error for container " & c.ContainerId & " " & OleDBExp.Message)
                                bolerror = True
                            End Try
                        Next
                    End If

                    ' hard write every 5 records
                    If flushCtr > 4 Then
                        objXMLWriter.Flush()
                        flushCtr = 0
                    Else
                        flushCtr += 1
                    End If
                Else
                    ' since FAB order, skip but log
                    m_logger.WriteInformationLog(rtn & " :: skipping FAB order : " & currOrder.OrderNo & ".")
                End If  'If Not currorder.IsFABOrder Then
            Next
        End If

        'objXMLWriter.WriteEndElement()
        'objXMLWriter.WriteEndElement()
        objXMLWriter.Flush()
        objXMLWriter.Close()

        Try
            Dim strXMLResult As String
            Dim objSR As StreamReader = File.OpenText(strXMLPath)
            strXMLResult = objSR.ReadToEnd()
            objSR.Close()
            objSR = Nothing
        Catch ex As Exception
            m_logger.WriteInformationLog(rtn & " :: error reading this XML file: " & strXMLPath & ".")
            bolerror = True
        End Try

        If bolerror = True Then
            Return True
        Else
            Return False
        End If
    End Function


    Private Sub SendEmail(ByVal bolError As Boolean)

        Dim rtn As String = "Module1.SendEmail"
        Dim email As New System.Web.Mail.MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "michael.randall@sdi.com; vitaly.rovensky@sdi.com"

        'The subject of the email
        If bolError = True Then
            email.Subject = "Shipping XML OUT Error(s) for I0260"
        Else
            email.Subject = "Shipping XML has completed for I0260"
        End If

        'email.Subject = " (Test) " & email.Subject

        'The Priority attached and displayed for the email
        email.Priority = System.Web.Mail.MailPriority.High

        email.BodyFormat = MailFormat.Html

        If bolError = True Then
            email.Body = "<html><body><table><tr><td>ShipXMLOut260 has completed with errors, review log.</td></tr>"
        Else
            email.Body = "<html><body><table><tr><td>ShipXMLOut260 has completed.</td></tr>"
        End If

        email.Bcc = "webdev@sdi.com"
        email.Cc = " "

        'Send the email and handle any error that occurs
        Try

            SendEmail1(email)

            'UpdEmailOut.UpdEmailOut.UpdEmailOut(email.Subject, email.From, email.To, "", email.Bcc, "N", email.Body, connectOR)

        Catch ex As Exception
            m_logger.WriteErrorLog(rtn & " :: Error - the email was not sent -> [" & ex.ToString & "]")
        End Try

    End Sub

    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try

            SendLogger(mailer.Subject, mailer.Body, "SHIPXMLOUTI0260", "Mail", mailer.To, mailer.Cc, mailer.Bcc)

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

    '// returns an array of containerInfo
    Private Function retrieveContainerIds(ByVal demandSource As String, _
                                          ByVal orderNo As String) As ArrayList
        Dim arr As New ArrayList
        Try
            Dim cntr As containerInfo = Nothing
            Dim sql As String = "" & _
                                "SELECT CNTR.SHIP_CNTR_ID " & vbCrLf & _
                                "FROM PS_ISA_PICKING_CNT CNTR " & vbCrLf & _
                                "WHERE CNTR.DEMAND_SOURCE = '" & demandSource & "' " & vbCrLf & _
                                "  AND CNTR.ORDER_NO = '" & orderNo & "' " & vbCrLf & _
                                "GROUP BY CNTR.SHIP_CNTR_ID " & vbCrLf & _
                                "ORDER BY CNTR.SHIP_CNTR_ID " & vbCrLf & _
                                ""
            Dim rdr As OleDbDataReader = Nothing
            Try
                rdr = ORDBAccess.GetReader(sql, connectOR)
            Catch OleDBExp As OleDbException
            End Try
            If Not (rdr Is Nothing) Then
                While rdr.Read
                    Dim cntrId As String = ""
                    Try
                        cntrId = CStr(rdr("SHIP_CNTR_ID")).Trim
                    Catch ex As Exception
                        cntrId = ""
                    End Try
                    If cntrId.Length > 0 Then
                        cntr = New containerInfo
                        cntr.ContainerId = cntrId
                        arr.Add(cntr)
                    End If
                End While
            End If
            Try
                rdr.Close()
            Catch ex43 As Exception

            End Try
        Catch ex As Exception
        End Try
        Try
            connectOR.Close()
        Catch ex As Exception
        End Try
        Return (arr)
    End Function

    '// returns an arraylist of orderShipmentInfo type of object
    '//     for non-stock orders
    Private Function processNSTKOrders() As processBatch
        Dim rtn As String = "Module1.processNSTKOrders"
        Dim nstkBatch As New processBatch

        Dim oraCN As New OleDbConnection(m_oraCNstring)
        oraCN.Open()

        Dim bu As String = "I0260"
        Dim dtStart As DateTime = m_nstkStartDate
        Dim dtEnd As DateTime = Now
        Dim arrOrders As ArrayList = getShippedNSTKOrders(bu, _
                                                          dtStart, _
                                                          dtEnd, _
                                                          oraCN)
        If arrOrders.Count > 0 Then
            Dim currOrder As orderShipmentInfo = Nothing
            Dim currOrderLine As orderLineInfo = Nothing

            For Each ord As changedOrder In arrOrders
                '// grab order data
                Dim sql As String = "" & _
                                    "SELECT " & vbCrLf & _
                                    " X.BUSINESS_UNIT " & vbCrLf & _
                                    ",X.ORDER_NO " & vbCrLf & _
                                    ",X.LINE_NBR " & vbCrLf & _
                                    ",X.ORDER_INT_LINE_NO " & vbCrLf & _
                                    ",X.DEMAND_LINE_NO " & vbCrLf & _
                                    ",X.ISA_EMPLOYEE_ID " & vbCrLf & _
                                    ",X.ISA_EMPLOYEE_NAME " & vbCrLf & _
                                    ",X.ISA_EMPLOYEE_EMAIL " & vbCrLf & _
                                    ",X.ITEM_ID " & vbCrLf & _
                                    ",X.ITEM_DESC " & vbCrLf & _
                                    ",X.QTY_PICKED " & vbCrLf & _
                                    ",X.QTY_SHIPPED " & vbCrLf & _
                                    ",X.SHIP_DATE " & vbCrLf & _
                                    ",X.SHIPTO_ID " & vbCrLf & _
                                    ",X.ISA_MACHINE_NO " & vbCrLf & _
                                    ",X.ISA_WORK_ORDER_NO " & vbCrLf & _
                                    ",X.SHIPTO_LOC " & vbCrLf & _
                                    "FROM " & vbCrLf & _
                                    "(" & vbCrLf & _
                                    " SELECT " & vbCrLf & _
                                    "  I.BUSINESS_UNIT_OM AS BUSINESS_UNIT " & vbCrLf & _
                                    " ,A.REQ_ID AS ORDER_NO " & vbCrLf & _
                                    " ,B.LINE_NBR AS LINE_NBR " & vbCrLf & _
                                    " ,0 AS ORDER_INT_LINE_NO " & vbCrLf & _
                                    " ,0 AS DEMAND_LINE_NO " & vbCrLf & _
                                    " ,B.ISA_EMPLOYEE_ID AS ISA_EMPLOYEE_ID " & vbCrLf & _
                                    " ,(C.FIRST_NAME_SRCH || ' ' || C.LAST_NAME_SRCH) AS ISA_EMPLOYEE_NAME " & vbCrLf & _
                                    " ,C.ISA_EMPLOYEE_EMAIL AS ISA_EMPLOYEE_EMAIL " & vbCrLf & _
                                    " ,' ' AS ITEM_ID " & vbCrLf & _
                                    " ,B.DESCR254_MIXED AS ITEM_DESC " & vbCrLf & _
                                    " ,H.QTY_LN_ACCPT_VUOM AS QTY_PICKED " & vbCrLf & _
                                    " ,H.QTY_LN_ACCPT_VUOM AS QTY_SHIPPED " & vbCrLf & _
                                    " ,H.RECEIPT_DT AS SHIP_DATE " & vbCrLf & _
                                    " ,H.SHIPTO_ID AS SHIPTO_ID " & vbCrLf & _
                                    " ,B.ISA_MACHINE_NO AS ISA_MACHINE_NO " & vbCrLf & _
                                    " ,B.ISA_WORK_ORDER_NO AS ISA_WORK_ORDER_NO " & vbCrLf & _
                                    " ,(" & vbCrLf & _
                                    "    SELECT LOC.DESCR AS DESCR " & vbCrLf & _
                                    "    FROM PS_LOCATION_TBL LOC" & vbCrLf & _
                                    "    WHERE LOC.SETID='MAIN1'" & vbCrLf & _
                                    "      AND LOC.EFF_STATUS = 'A'" & vbCrLf & _
                                    "      AND LOC.LOCATION = H.SHIPTO_ID " & vbCrLf & _
                                    "      AND LOC.EFFDT = (" & vbCrLf & _
                                    "                       SELECT MAX(LOC1.EFFDT)" & vbCrLf & _
                                    "                       FROM PS_LOCATION_TBL LOC1" & vbCrLf & _
                                    "                       WHERE LOC1.SETID = LOC.SETID" & vbCrLf & _
                                    "                         AND LOC1.LOCATION = LOC.LOCATION" & vbCrLf & _
                                    "                         AND LOC1.EFF_STATUS = LOC.EFF_STATUS" & vbCrLf & _
                                    "                         AND LOC1.EFFDT <= SYSDATE" & vbCrLf & _
                                    "                      )" & vbCrLf & _
                                    "      AND ROWNUM < 2" & vbCrLf & _
                                    "  ) AS SHIPTO_LOC" & vbCrLf & _
                                    " FROM " & vbCrLf & _
                                    "  PS_REQ_HDR A " & vbCrLf & _
                                    " ,PS_REQ_LINE B " & vbCrLf & _
                                    " ,(" & vbCrLf & _
                                    "   SELECT " & vbCrLf & _
                                    "    USR.ISA_USER_ID " & vbCrLf & _
                                    "   ,USR.FIRST_NAME_SRCH " & vbCrLf & _
                                    "   ,USR.LAST_NAME_SRCH " & vbCrLf & _
                                    "   ,USR.BUSINESS_UNIT " & vbCrLf & _
                                    "   ,USR.ISA_EMPLOYEE_ID " & vbCrLf & _
                                    "   ,USR.ISA_EMPLOYEE_NAME " & vbCrLf & _
                                    "   ,USR.ISA_EMPLOYEE_EMAIL " & vbCrLf & _
                                    "   FROM PS_ISA_USERS_TBL USR " & vbCrLf & _
                                    "   WHERE USR.BUSINESS_UNIT = '" & ord.BusinessUnit & "' " & vbCrLf & _
                                    "  ) C " & vbCrLf & _
                                    " ,(" & vbCrLf & _
                                    "   SELECT " & vbCrLf & _
                                    "    H1.BUSINESS_UNIT" & vbCrLf & _
                                    "   ,H1.REQ_ID" & vbCrLf & _
                                    "   ,H1.REQ_LINE_NBR" & vbCrLf & _
                                    "   ,SUM(H1.QTY_PO) AS QTY_PO" & vbCrLf & _
                                    "   ,SUM(NVL(H2.QTY_LN_ACCPT_VUOM,0)) AS QTY_LN_ACCPT_VUOM" & vbCrLf & _
                                    "   ,MAX(H3.RECEIPT_DT) AS RECEIPT_DT" & vbCrLf & _
                                    "   ,(" & vbCrLf & _
                                    "     SELECT H11.LOCATION" & vbCrLf & _
                                    "     FROM PS_PO_LINE_DISTRIB H11" & vbCrLf & _
                                    "     WHERE H11.BUSINESS_UNIT = H1.BUSINESS_UNIT" & vbCrLf & _
                                    "       AND H11.REQ_ID = H1.REQ_ID" & vbCrLf & _
                                    "       AND H11.REQ_LINE_NBR = H1.REQ_LINE_NBR" & vbCrLf & _
                                    "       AND H11.DISTRIB_LN_STATUS <> 'X'" & vbCrLf & _
                                    "       AND ROWNUM < 2" & vbCrLf & _
                                    "     GROUP BY H11.LOCATION " & vbCrLf & _
                                    "    ) AS SHIPTO_ID" & vbCrLf & _
                                    "   FROM PS_PO_LINE_DISTRIB H1" & vbCrLf & _
                                    "   ,PS_RECV_LN H2" & vbCrLf & _
                                    "   ,PS_RECV_HDR H3" & vbCrLf & _
                                    "   WHERE H1.BUSINESS_UNIT = 'ISA00'" & vbCrLf & _
                                    "     AND H1.REQ_ID = '" & ord.OrderNo & "' " & vbCrLf & _
                                    "     AND H1.DISTRIB_LN_STATUS <> 'X'" & vbCrLf & _
                                    "     AND H1.BUSINESS_UNIT = H2.BUSINESS_UNIT_PO (+)" & vbCrLf & _
                                    "     AND H1.PO_ID = H2.PO_ID (+)" & vbCrLf & _
                                    "     AND H1.LINE_NBR = H2.LINE_NBR (+)" & vbCrLf & _
                                    "     AND H2.BUSINESS_UNIT = H3.BUSINESS_UNIT (+)" & vbCrLf & _
                                    "     AND H2.RECEIVER_ID = H3.RECEIVER_ID (+)" & vbCrLf & _
                                    "   GROUP BY " & vbCrLf & _
                                    "    H1.BUSINESS_UNIT" & vbCrLf & _
                                    "   ,H1.REQ_ID" & vbCrLf & _
                                    "   ,H1.REQ_LINE_NBR" & vbCrLf & _
                                    "   ORDER BY " & vbCrLf & _
                                    "    H1.BUSINESS_UNIT" & vbCrLf & _
                                    "   ,H1.REQ_ID" & vbCrLf & _
                                    "   ,H1.REQ_LINE_NBR" & vbCrLf & _
                                    "  ) H" & vbCrLf & _
                                    " ,PS_ISA_ORD_INTFC_H I " & vbCrLf & _
                                    " WHERE A.BUSINESS_UNIT = 'ISA00' " & vbCrLf & _
                                    "   AND A.REQ_ID = '" & ord.OrderNo & "' " & vbCrLf & _
                                    "   AND A.BUSINESS_UNIT = B.BUSINESS_UNIT " & vbCrLf & _
                                    "   AND A.REQ_ID = B.REQ_ID " & vbCrLf & _
                                    "   AND '" & ord.BusinessUnit & "' = I.BUSINESS_UNIT_OM " & vbCrLf & _
                                    "   AND A.REQ_ID = I.ORDER_NO " & vbCrLf & _
                                    "   AND '" & ord.BusinessUnit & "' = C.BUSINESS_UNIT (+) " & vbCrLf & _
                                    "   AND B.ISA_EMPLOYEE_ID = C.ISA_EMPLOYEE_ID (+) " & vbCrLf & _
                                    "   AND A.REQ_ID = H.REQ_ID " & vbCrLf & _
                                    "   AND B.LINE_NBR = H.REQ_LINE_NBR " & vbCrLf & _
                                    "   AND NOT EXISTS (" & vbCrLf & _
                                    "                   SELECT 'X'" & vbCrLf & _
                                    "                   FROM PS_ISA_PICKING_SHP G " & vbCrLf & _
                                    "                   WHERE '" & ord.OrderNo & nstkContainerIdSuffix & "' = G.SHIP_CNTR_ID " & vbCrLf & _
                                    "                  )" & vbCrLf & _
                                    ") X" & vbCrLf & _
                                    "ORDER BY X.BUSINESS_UNIT, X.ORDER_NO, X.ISA_EMPLOYEE_ID, X.LINE_NBR " & vbCrLf & _
                                    ""

                Dim cmd As OleDbCommand = oraCN.CreateCommand
                cmd.CommandText = sql
                cmd.CommandType = CommandType.Text

                Dim rdr As OleDbDataReader = Nothing

                'm_logger.WriteVerboseLog(rtn & " :: executing : " & vbCrLf & sql)
                Try
                    rdr = cmd.ExecuteReader
                Catch ex As Exception
                End Try

                If Not (rdr Is Nothing) Then
                    If Not rdr.HasRows Then
                        m_logger.WriteVerboseLog(rtn & " :: order # " & ord.OrderNo & " may have already been processed/sent for I0260 version.")
                    End If

                    Dim demandSrc As String = ""
                    Dim orderNo As String = ""
                    Dim orderLn As Integer = -1
                    Dim workOrderNo As String = ""
                    Dim machineNo As String = ""
                    Dim qty As Decimal = 0
                    Dim shipDT As String = ""

                    While rdr.Read
                        orderNo = ""
                        Try
                            orderNo = CStr(rdr("ORDER_NO"))
                        Catch ex As Exception
                        End Try

                        orderLn = -1
                        Try
                            orderLn = CInt(rdr("LINE_NBR"))
                        Catch ex As Exception
                        End Try

                        qty = 0
                        Try
                            qty = CDec(rdr("QTY_PICKED"))
                        Catch ex As Exception
                        End Try

                        shipDT = ""
                        Try
                            shipDT = CDate(rdr("SHIP_DATE")).ToString
                        Catch ex As Exception
                        End Try

                        demandSrc = "OM"

                        workOrderNo = ""
                        Try
                            workOrderNo = CStr(rdr("ISA_WORK_ORDER_NO")).Trim
                        Catch ex As Exception
                        End Try

                        machineNo = ""
                        Try
                            machineNo = CStr(rdr("ISA_MACHINE_NO")).Trim.ToUpper
                        Catch ex As Exception
                        End Try

                        currOrder = Nothing

                        If nstkBatch.OrderNoList.IndexOf(orderNo) > -1 Then
                            ' order already added
                            currOrder = nstkBatch.GetOrder(orderNo)
                            currOrder.DemandSource = demandSrc
                        Else
                            ' order not found in collection yet
                            currOrder = New orderShipmentInfo
                            currOrder.DemandSource = demandSrc
                            currOrder.OrderNo = orderNo
                            currOrder.Shipper = "MoveWay"
                            currOrder.IsFABOrder = (machineNo = orderType_FAB)
                            nstkBatch.Orders.Add(currOrder)
                        End If

                        If Not (currOrder Is Nothing) Then
                            currOrderLine = New orderLineInfo
                            currOrderLine.Parent = currOrder
                            currOrderLine.LineNo = orderLn
                            currOrderLine.QuantityPicked = qty
                            currOrderLine.ShipDateTime = shipDT
                            currOrderLine.WorkOrderNo = workOrderNo
                            If Not currOrder.IsFABOrder And _
                               (machineNo = orderType_FAB) Then
                                currOrder.IsFABOrder = (machineNo = orderType_FAB)
                            End If
                            currOrder.OrderLines.Add(currOrderLine)
                        Else
                            ' what to do ???
                        End If  'If not (currorder Is Nothing ) Then
                    End While
                End If

                Try
                    rdr.Close()
                Catch ex As Exception
                Finally
                    rdr = Nothing
                End Try

                Try
                    cmd.Dispose()
                Catch ex As Exception
                Finally
                    cmd = Nothing
                End Try

            Next
        End If

        Try
            oraCN.Close()
        Catch ex As Exception
        End Try
        Try
            oraCN.Dispose()
        Catch ex As Exception
        Finally
            oraCN = Nothing
        End Try

        Return (nstkBatch)
    End Function

    Private Function getShippedNSTKOrders(ByVal bu As String, _
                                          ByVal dtStart As DateTime, _
                                          ByVal dtEnd As DateTime, _
                                          ByVal oraCN As OleDbConnection) As ArrayList
        Dim rtn As String = "Module1.getShippedNSTKOrders"
        Const ordStatusLog_SHIPPED As String = "6"
        Dim arr As New ArrayList

        Dim sql As String = "" & vbCrLf & _
                            "SELECT " & vbCrLf & _
                            " A.ORDER_NO " & vbCrLf & _
                            ",A.BUSINESS_UNIT_OM " & vbCrLf & _
                            "FROM PS_ISAORDSTATUSLOG A" & vbCrLf & _
                            "WHERE (A.DTTM_STAMP BETWEEN TO_DATE('" & dtStart.ToString("MM/dd/yyyy HH:mm:ss") & "','MM/DD/YYYY HH24:MI:SS') AND TO_DATE('" & dtEnd.ToString("MM/dd/yyyy HH:mm:ss") & "','MM/DD/YYYY HH24:MI:SS')) " & vbCrLf & _
                            "  AND A.BUSINESS_UNIT_OM = '" & bu & "' " & vbCrLf & _
                            "  AND A.ISA_ORDER_STATUS = '" & ordStatusLog_SHIPPED & "' " & vbCrLf & _
                            "  AND NOT EXISTS (" & vbCrLf & _
                            "                  SELECT 'X'" & vbCrLf & _
                            "  		           FROM PS_ORD_HEADER B" & vbCrLf & _
                            "                  WHERE B.BUSINESS_UNIT = A.BUSINESS_UNIT_OM" & vbCrLf & _
                            "                    AND B.ORDER_NO = A.ORDER_NO" & vbCrLf & _
                            "                 )" & vbCrLf & _
                            "  AND NOT EXISTS (" & vbCrLf & _
                            "                  SELECT 'X'" & vbCrLf & _
                            "                  FROM PS_PO_LINE_DISTRIB C" & vbCrLf & _
                            "                  WHERE C.BUSINESS_UNIT = 'ISA00'" & vbCrLf & _
                            "                    AND C.REQ_ID = A.ORDER_NO" & vbCrLf & _
                            "                    AND C.DISTRIB_LN_STATUS <> 'X'" & vbCrLf & _
                            "                    AND C.QTY_PO > (" & vbCrLf & _
                            "                                    SELECT SUM(NVL(E.QTY_LN_ACCPT_VUOM,0)) AS QTY_LN_ACCPT_VUOM" & vbCrLf & _
                            "                                    FROM PS_RECV_LN E" & vbCrLf & _
                            "                                    WHERE C.BUSINESS_UNIT = E.BUSINESS_UNIT_PO" & vbCrLf & _
                            "                                      AND C.PO_ID         = E.PO_ID" & vbCrLf & _
                            "                                      AND C.LINE_NBR      = E.LINE_NBR" & vbCrLf & _
                            "                                   )" & vbCrLf & _
                            "                 )" & vbCrLf & _
                            "GROUP BY A.ORDER_NO, A.BUSINESS_UNIT_OM " & vbCrLf & _
                            ""
        Dim cmd As OleDbCommand = oraCN.CreateCommand
        cmd.CommandText = sql
        cmd.CommandType = CommandType.Text

        Dim rdr As OleDbDataReader = Nothing

        m_logger.WriteVerboseLog(rtn & " :: executing : " & vbCrLf & sql)
        Try
            rdr = cmd.ExecuteReader
        Catch ex As Exception
        End Try

        If Not (rdr Is Nothing) Then
            Dim sBU As String = ""
            Dim sOrder As String = ""
            While rdr.Read
                sBU = ""
                Try
                    sBU = CStr(rdr("BUSINESS_UNIT_OM")).Trim.ToUpper
                Catch ex As Exception
                End Try
                sOrder = ""
                Try
                    sOrder = CStr(rdr("ORDER_NO")).Trim.ToUpper
                Catch ex As Exception
                End Try
                If sBU.Length > 0 And sOrder.Length > 0 Then
                    arr.Add(New changedOrder(sBU, sOrder))
                End If
            End While
        End If

        Try
            rdr.Close()
        Catch ex As Exception
        Finally
            rdr = Nothing
        End Try

        Try
            cmd.Dispose()
        Catch ex As Exception
        Finally
            cmd = Nothing
        End Try

        Return (arr)
    End Function

End Module
