Imports System.Xml
Imports System.IO
Imports System.Data.OleDb
Imports System.Web.Mail
Imports System.Collections.Generic


Module Module1

    Private m_logger As ApplicationLogger = Nothing

    Private Const oraCN_default_provider As String = "Provider=OraOLEDB.Oracle.1;"
    Private Const oraCN_default_creden As String = "User ID=einternet;Password=einternet;"
    Private Const oraCN_default_DB As String = "Data Source=PROD;"

    Private Const orderType_FAB As String = "FAB"

    Dim rootDir As String = "C:\SHIPXML\XMLOut260FAB"
    Dim logpath As String = "C:\SHIPXML\XMLOut260FAB\LOGS\UpdSHIPXMLOutFAB" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

    Private connectOR As OleDbConnection = Nothing

    Private m_oraCNstring As String = "" & _
                                      oraCN_default_provider & _
                                      oraCN_default_creden & _
                                      oraCN_default_DB & _
                                      ""
    Dim objStreamWriter As StreamWriter

    Sub Main()

        Dim rtn As String = "Module1.Main"

        Console.WriteLine("Start Shipping XML out for 260 (FAB)")
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
            Dim db As String = CStr(param(key:="/DB")).Trim.ToUpper
            m_oraCNstring = "" & _
                            oraCN_default_provider & _
                            oraCN_default_creden & _
                            "Data Source=" & db & ";" & _
                            ""
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
        m_logger.WriteInformationLog(rtn & " :: Start of SP Processing XML out I0260 FAB version.")
        m_logger.WriteVerboseLog(rtn & " :: connection string : [" & connectOR.ConnectionString & "]")

        m_logger.WriteInformationLog(rtn & " :: Update of SP Processing XML out.")

        Dim bolError As Boolean = buildPickingXMLOut()

        m_logger.WriteInformationLog(rtn & " :: Sending email notification.")

        SendEmail(bolError)

        ' final log for this run
        m_logger.WriteInformationLog(rtn & " :: Ending XML out process.")

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

    Private Function buildPickingXMLOut() As Boolean

        Dim rtn As String = "Module1.buildPickingXMLOut"
        Dim bolerror As Boolean = False

        '"    AND PICK1.SEQ_NBR = CNTR1.SEQ_NBR (+) " & vbCrLf & _

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
",MAX(A.ENDDTTM) AS PICKED_DTTM " & vbCrLf & _
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
"    and pick1.ENDDTTM IS NOT NULL " & vbCrLf & _
"    and pick1.BEGINDTTM > trunc(sysdate-120) " & vbCrLf & _
"    AND PICK1.BUSINESS_UNIT = CNTR1.BUSINESS_UNIT (+) " & vbCrLf & _
"    AND PICK1.DEMAND_SOURCE = CNTR1.DEMAND_SOURCE (+) " & vbCrLf & _
"    AND PICK1.SOURCE_BUS_UNIT = CNTR1.SOURCE_BUS_UNIT (+) " & vbCrLf & _
"    AND PICK1.ORDER_NO = CNTR1.ORDER_NO (+) " & vbCrLf & _
"    AND PICK1.ORDER_INT_LINE_NO = CNTR1.ORDER_INT_LINE_NO (+) " & vbCrLf & _
"    AND PICK1.SCHED_LINE_NO = CNTR1.SCHED_LINE_NO (+) " & vbCrLf & _
"    AND PICK1.INV_ITEM_ID = CNTR1.INV_ITEM_ID (+) " & vbCrLf & _
"    AND PICK1.DEMAND_LINE_NO = CNTR1.DEMAND_LINE_NO (+) " & vbCrLf & _
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
"                         AND (A.QTY_PICKED = 0 OR A.ENDDTTM IS NULL)" & vbCrLf & _
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
            m_logger.WriteErrorLog(rtn & " :: Error - error reading transaction FROM PS_ISA_PICKING_CNT A, PS_ISA_PICKING_INT B")
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
                    ' grab when this order got picked and not shipped when dealing with FAB orders
                    'shipDT = CDate(rdr("SHIP_DTTM")).ToString
                    shipDT = CDate(rdr("PICKED_DTTM")).ToString
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
            m_logger.WriteWarningLog(rtn & " :: Warning - no containers to process at this time.")
            Return True
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

        Dim I As Integer
        Dim rowsaffected As Integer
        Dim dteJulian As Integer
        Dim dteStart As Date = "01/01/1900"

        dteJulian = DateDiff(DateInterval.Day, dteStart, Now())

        Dim runDT As DateTime = Now
        Dim strXMLPath As String = rootDir & "\XMLOUT\DOEFAB" & runDT.ToString(format:="MMddyy_HHmmss") & ".txt"
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
                '   this is the other half of the process WHICH WILL ONLY INCLUDE FAB orders
                '   - erwin 2010.05.24
                If currOrder.IsFABOrder Then
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
                    ' since not FAB order, skip but log
                    m_logger.WriteInformationLog(rtn & " :: skipping NON-FAB order : " & currOrder.OrderNo & ".")
                End If  'If Not currorder.IsFABOrder Then
            Next
        End If

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


    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try

            SendLogger(mailer.Subject, mailer.Body, "SHIPXMLOUTI0260FAB", "Mail", mailer.To, mailer.Cc, mailer.Bcc)

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


    Private Sub SendEmail(ByVal bolError As Boolean)

        Dim rtn As String = "Module1.SendEmail"
        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "vitaly.rovensky@sdi.com"

        email.Bcc = "webdev@sdi.com"
        email.Cc = ""

        'The subject of the email
        If bolError = True Then
            email.Subject = "Shipping XML OUT Error for I0260 (FAB)"
        Else
            email.Subject = "Shipping XML has completed for I0260 (FAB)"
        End If

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        If bolError = True Then
            email.Body = "<html><body><table><tr><td>ShipXMLOut FAB has completed with errors, review log.</td></tr>"
        Else
            email.Body = "<html><body><table><tr><td>ShipXMLOut FAB has completed.</td></tr>"
        End If

        'Send the email and handle any error that occurs
        Try

            SendEmail1(email)

            'UpdEmailOut.UpdEmailOut.UpdEmailOut(email.Subject, email.From, email.To, "", email.Bcc, "N", email.Body, connectOR)

        Catch ex As Exception

            m_logger.WriteErrorLog(rtn & " :: Error - the email was not sent -> [" & ex.ToString & "]")
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
        Catch ex As Exception
        End Try
        Try
            connectOR.Close()
        Catch ex As Exception
        End Try
        Return (arr)
    End Function

End Module
