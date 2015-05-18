Imports System.Xml
Imports System.IO
Imports System.Data.OleDb
Imports System.Web.Mail
Imports FTP


Module Module1

    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\UofPXML"
    Dim logpath As String = "C:\UofPXML\LOGS\getUofPBilXMLOut" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=PROD")
    Dim dtUOM As DataTable
    Dim dtEmp As DataTable

    Sub Main()

        Console.WriteLine("Start UofPBilXML XML out")
        Console.WriteLine("")

        If Dir(rootDir, FileAttribute.Directory) = "" Then
            MkDir(rootDir)
        End If
        If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\LOGS")
        End If
        If Dir(rootDir & "\XMLOUT", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\XMLOUT")
        End If
        If Dir(rootDir & "\XMLOUTProcessed", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\XMLOUTProcessed")
        End If

        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("Update of UofPBilXML Processing XML out " & Now())

        Dim bolError As Boolean = buildUofPBilXMLOut()
        'If bolError = False Then
        '    sendFTPfiles()
        'End If
        sendFTPfiles()

        SendEmail(bolError)

        objStreamWriter.WriteLine("End of UofPXML Processing XML out " & Now())
        objStreamWriter.Flush()
        objStreamWriter.Close()

    End Sub

    Private Function buildUofPBilXMLOut() As Boolean

        Dim ds As New DataSet
        Dim bolerror As Boolean
        Dim strSQLstring As String = "SELECT /*+ RULE */" & vbCrLf & _
                " TO_CHAR(B.SHIP_DATE,'YYYY-MM-DD') as SHIP_DT," & vbCrLf & _
                " TO_CHAR(A.DT_INVOICED,'YYYY-MM-DD') as INVOICE_DT," & vbCrLf & _
                " A.BILL_TO_CUST_ID," & vbCrLf & _
                " c.ISA_CUST_CHARGE_CD," & vbCrLf & _
                " c.ISA_WORK_ORDER_NO," & vbCrLf & _
                " c.ISA_MACHINE_NO," & vbCrLf & _
                " c.ISA_EMPLOYEE_ID," & vbCrLf & _
                " B.BUSINESS_UNIT," & vbCrLf & _
                " B.INVOICE, B.LINE_SEQ_NUM, B.INVOICE_LINE," & vbCrLf & _
                " B.NET_EXTENDED_AMT," & vbCrLf & _
                " B.ORDER_NO," & vbCrLf & _
                " B.PO_REF," & vbCrLf & _
                " B.DESCR," & vbCrLf & _
                " B.GROSS_EXTENDED_AMT," & vbCrLf & _
                " 'SDI' || TO_CHAR(SYSDATE,'MMDDYY') as BATCH_NUMBER," & vbCrLf & _
                " B.PRODUCT_ID," & vbCrLf & _
                " B.QTY," & vbCrLf & _
                " B.UNIT_OF_MEASURE," & vbCrLf & _
                " d.TL_COST" & vbCrLf & _
                " FROM sysadm8.PS_BI_HDR A," & vbCrLf & _
                " sysadm8.ps_BI_LINE B," & vbCrLf & _
                " sysadm8.ps_isa_bi_line c," & vbCrLf & _
                " ps_cm_prodcost d" & vbCrLf & _
                " WHERE A.BUSINESS_UNIT   = 'ISASM'" & vbCrLf & _
                " AND   A.BILL_TO_CUST_ID = '90321'" & vbCrLf & _
                " AND   A.BUSINESS_UNIT   = B.BUSINESS_UNIT" & vbCrLf & _
                " AND   A.INVOICE         = B.INVOICE" & vbCrLf & _
                " and   B.ship_date > TO_DATE('2007-07-23','YYYY-MM-DD')" & vbCrLf & _
                " and   b.business_unit   = c.business_unit" & vbCrLf & _
                " and   b.invoice         = c.invoice" & vbCrLf & _
                " and   b.line_seq_num   = c.line_seq_num" & vbCrLf & _
                " and   c.ISA_CUST_CHARGE_CD not in ('RECOVERY')" & vbCrLf & _
                " and   d.BUSINESS_UNIT(+) = 'C0206'" & vbCrLf & _
                " and   d.INV_ITEM_ID(+) = B.IDENTIFIER" & vbCrLf & _
                " AND ( d.EFFDT =" & vbCrLf & _
                " (SELECT MAX(A_ED.EFFDT) FROM PS_CM_PRODCOST A_ED" & vbCrLf & _
                " WHERE(d.BUSINESS_UNIT = A_ED.BUSINESS_UNIT)" & vbCrLf & _
                " AND d.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf & _
                " AND d.CONFIG_CODE = A_ED.CONFIG_CODE" & vbCrLf & _
                " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                " OR d.EFFDT IS NULL)" & vbCrLf & _
                " and NOT EXISTS(SELECT 'X'" & vbCrLf & _
                " FROM PS_ISA_BILL_TRAN BILLTRAN" & vbCrLf & _
                " WHERE BILLTRAN.BUSINESS_UNIT = A.BUSINESS_UNIT" & vbCrLf & _
                " AND BILLTRAN.INVOICE = B.INVOICE" & vbCrLf & _
                " AND BILLTRAN.LINE_SEQ_NUM = B.LINE_SEQ_NUM)" & vbCrLf & _
                " and NOT EXISTS(SELECT 'X'" & vbCrLf & _
                " from  PS_ISA_BUR_TBL BUR " & vbCrLf & _
                " WHERE (Bur.PO_ID = B.PO_REF)" & vbCrLf & _
                " AND Bur.LINE_NBR = B.PO_LINE)" & vbCrLf


        '" and   B.ship_date between TO_DATE('2007-08-24','YYYY-MM-DD')" & vbCrLf & _
        '       " and TO_DATE('2007-09-21','YYYY-MM-DD')" & vbCrLf & _


        '" and   B.ship_date > TO_DATE('2007-07-23','YYYY-MM-DD')" & vbCrLf & _

        '" and   B.ship_date between TO_DATE('2007-07-23','YYYY-MM-DD')" & vbCrLf & _
        '" and TO_DATE('2007-08-23','YYYY-MM-DD')" & vbCrLf & _

        '" and   B.ship_date > TO_DATE('2007-07-23','YYYY-MM-DD')" & vbCrLf & _

        '" aND   A.BILL_STATUS     = 'NEW'" & vbCrLf & _

        '" and   c.ISA_CUST_CHARGE_CD not in ('SLIPPAGE', 'RECOVERY')" & vbCrLf & _

        Try
            ds = ORDBAccess.GetAdapter(strSQLstring, connectOR)

        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            connectOR.Close()
            objStreamWriter.WriteLine("     Error - error reading transaction FROM Item tables")
            Return True
        End Try

        If ds.Tables(0).Rows.Count = 0 Then
            objStreamWriter.WriteLine("     Warning - no billing transactions to process at this time")
            Return True
        End If
        Dim I As Integer
        Dim rowsaffected As Integer
        Dim dteJulian As Integer
        Dim dteStart As Date = "01/01/1900"
        dteJulian = DateDiff(DateInterval.Day, dteStart, Now())

        Dim strXMLPath As String = rootDir & "\XMLOUT\updUofPBil" & Convert.ToString(dteJulian) & Now.Hour.ToString("D2") & Now.Minute.ToString("D2") & Now.Second.ToString("D2") & ".xml"
        Dim objXMLWriter As XmlTextWriter
        Try
            objXMLWriter = New XmlTextWriter(strXMLPath, Nothing)
            objStreamWriter.WriteLine("  Writing to file: " & strXMLPath)
        Catch objError As Exception
            objStreamWriter.WriteLine("  Error while accessing document " & strXMLPath & vbCrLf & objError.Message)
            Return True
        End Try
        objXMLWriter.Formatting = Formatting.Indented
        objXMLWriter.Indentation = 3
        objXMLWriter.WriteStartDocument()
        objXMLWriter.WriteComment("Created on " & Now())
        objXMLWriter.WriteStartElement("SDI")

        buildUOMTable()
        buildEmpTable()

        connectOR.Open()

        Dim strUOM As String
        Dim strEmployeeID As String
        Dim strCustChargeCD As String
        Dim strWorkOrderNO As String

        For I = 0 To ds.Tables(0).Rows.Count - 1
            objXMLWriter.WriteStartElement("Row")
            objXMLWriter.WriteAttributeString("id", (I + 1))
            If IsDBNull(ds.Tables(0).Rows(I).Item("SHIP_DT")) Then
                objXMLWriter.WriteElementString("SHIP_DT", " ")
            Else
                objXMLWriter.WriteElementString("SHIP_DT", ds.Tables(0).Rows(I).Item("SHIP_DT"))
            End If

            strCustChargeCD = ds.Tables(0).Rows(I).Item("ISA_CUST_CHARGE_CD")
            strWorkOrderNO = ds.Tables(0).Rows(I).Item("ISA_WORK_ORDER_NO")
            'If strCustChargeCD.ToUpper = "SLIPPAGE" Or _
            '    strCustChargeCD.ToUpper = "RECOVERY" Then
            If strCustChargeCD.ToUpper = "SLIPPAGE" Then
                'strCustChargeCD = "08-001444"
                strCustChargeCD = "08-001444-002"
                strWorkOrderNO = "08-001444"
            End If
            objXMLWriter.WriteElementString("ISA_CUST_CHARGE_CD", strCustChargeCD)
            objXMLWriter.WriteElementString("ISA_WORK_ORDER_NO", strWorkOrderNO)
            objXMLWriter.WriteElementString("ISA_MACHINE_NO", ds.Tables(0).Rows(I).Item("ISA_MACHINE_NO"))
            strEmployeeID = getEmpID(ds.Tables(0).Rows(I).Item("ISA_EMPLOYEE_ID"))
            objXMLWriter.WriteElementString("ISA_EMPLOYEE_ID", strEmployeeID)
            objXMLWriter.WriteElementString("NET_EXTENDED_AMT", ds.Tables(0).Rows(I).Item("NET_EXTENDED_AMT"))
            'objXMLWriter.WriteElementString("NET_EXTENDED_AMT", (FormatNumber((ds.Tables(0).Rows(I).Item("NET_EXTENDED_AMT") + _
            '                ds.Tables(0).Rows(I).Item("NET_EXTENDED_AMT") * 0.12), 2)))
            objXMLWriter.WriteElementString("ORDER_NO", ds.Tables(0).Rows(I).Item("ORDER_NO"))
            objXMLWriter.WriteElementString("PO_REF", ds.Tables(0).Rows(I).Item("PO_REF"))
            objXMLWriter.WriteElementString("DESCR", ds.Tables(0).Rows(I).Item("DESCR"))
            objXMLWriter.WriteElementString("INVOICE", ds.Tables(0).Rows(I).Item("INVOICE"))
            If IsDBNull(ds.Tables(0).Rows(I).Item("INVOICE_DT")) Then
                objXMLWriter.WriteElementString("INVOICE_DT", "")
            Else
                objXMLWriter.WriteElementString("INVOICE_DT", ds.Tables(0).Rows(I).Item("INVOICE_DT"))
            End If

            objXMLWriter.WriteElementString("GROSS_EXTENDED_AMT", ds.Tables(0).Rows(I).Item("GROSS_EXTENDED_AMT"))
            'objXMLWriter.WriteElementString("GROSS_EXTENDED_AMT", (FormatNumber((ds.Tables(0).Rows(I).Item("GROSS_EXTENDED_AMT") + _
            '                ds.Tables(0).Rows(I).Item("GROSS_EXTENDED_AMT") * 0.12), 2)))
            objXMLWriter.WriteElementString("BATCH_NUMBER", ds.Tables(0).Rows(I).Item("BATCH_NUMBER"))
            objXMLWriter.WriteElementString("PRODUCT_ID", ds.Tables(0).Rows(I).Item("PRODUCT_ID"))
            objXMLWriter.WriteElementString("QTY", ds.Tables(0).Rows(I).Item("QTY"))
            strUOM = getUOM(ds.Tables(0).Rows(I).Item("UNIT_OF_MEASURE"))
            objXMLWriter.WriteElementString("UNIT_OF_MEASURE", strUOM)
            If IsDBNull(ds.Tables(0).Rows(I).Item("TL_COST")) Then
                objXMLWriter.WriteElementString("TL_COST", "0")
            Else
                objXMLWriter.WriteElementString("TL_COST", ds.Tables(0).Rows(I).Item("TL_COST"))
                'objXMLWriter.WriteElementString("TL_COST", (FormatNumber((ds.Tables(0).Rows(I).Item("TL_COST") + _
                '                    ds.Tables(0).Rows(I).Item("TL_COST") * 0.12), 2)))
            End If
            objXMLWriter.WriteEndElement()

            bolerror = insertBillTran(ds.Tables(0).Rows(I))

        Next

        objXMLWriter.WriteEndElement()
        objXMLWriter.Flush()
        objXMLWriter.Close()

        connectOR.Close()
        Dim strXMLResult As String
        Dim objSR As StreamReader = File.OpenText(strXMLPath)
        strXMLResult = objSR.ReadToEnd()
        objSR.Close()
        objSR = Nothing

        If bolerror = True Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub buildEmpTable()

        Dim reader As XmlTextReader
        Try
            reader = New XmlTextReader("XMLEMP.XML")
        Catch ex As Exception
            Console.WriteLine("")
            Console.WriteLine("***error - " & ex.ToString)
            Console.WriteLine("")
            objStreamWriter.WriteLine("  Error created XMLEMP" & ex.ToString)
        End Try

        Dim I As Integer
        Dim dsRows As New DataSet

        ' Read in XML from file
        Try
            dsRows.ReadXml(reader)
        Catch ex As Exception
            Console.WriteLine("")
            Console.WriteLine("***error - " & ex.ToString)
            Console.WriteLine("")
            objStreamWriter.WriteLine("  Error created XMLEMP Dataset" & ex.ToString)

        End Try

        reader.Close()
        dtEmp = dsRows.Tables(0)
    End Sub

    Private Sub buildUOMTable()

        Dim reader As XmlTextReader
        Try
            reader = New XmlTextReader("XMLUOM.XML")
        Catch ex As Exception
            Console.WriteLine("")
            Console.WriteLine("***error - " & ex.ToString)
            Console.WriteLine("")
            objStreamWriter.WriteLine("  Error created Delete XMLUOM" & ex.ToString)
        End Try

        Dim I As Integer
        Dim dsRows As New DataSet

        ' Read in XML from file
        Try
            dsRows.ReadXml(reader)
        Catch ex As Exception
            Console.WriteLine("")
            Console.WriteLine("***error - " & ex.ToString)
            Console.WriteLine("")
            objStreamWriter.WriteLine("  Error created Delete XML Dataset" & ex.ToString)

        End Try

        reader.Close()
        dtUOM = dsRows.Tables(0)
    End Sub

    Private Function getEmpID(ByVal strEmpID As String) As String

        Dim I As Integer
        getEmpID = strEmpID
        For I = 0 To dtEmp.Rows.Count - 1
            If dtEmp.Rows(I).Item("isa_employee_id") = strEmpID Then
                getEmpID = dtEmp.Rows(I).Item("shop_person")
                Exit For
            End If
        Next

    End Function


    Private Function getUOM(ByVal strUOM As String) As String

        Dim I As Integer
        For I = 0 To dtUOM.Rows.Count - 1
            If dtUOM.Rows(I).Item("SDIUOM") = strUOM Then
                getUOM = dtUOM.Rows(I).Item("UPENNUOM")
                Exit For
            End If
        Next
        If Trim(getUOM) = "" Then
            getUOM = "EA"
            objStreamWriter.WriteLine("  Warning - UOM not translated to UPenn UOM" & strUOM)

        End If

    End Function

    Private Function insertBillTran(ByVal dtrow As DataRow) As Boolean

        ' added to handle single quote on ISA_CUST_CHARGE_CD and ISA_WORK_ORDER_NO fields
        '   need to insert the single quote since the source will carry it.
        '   - erwin 2009.09.18
        Dim custChargeCode As String = CStr(dtrow.Item("ISA_CUST_CHARGE_CD"))
        custChargeCode = custChargeCode.Replace("'", "''")
        Dim custWorkOrderNo As String = CStr(dtrow.Item("ISA_WORK_ORDER_NO"))
        custWorkOrderNo = custWorkOrderNo.Replace("'", "''")

        Dim strSQLstring As String
        strSQLstring = "INSERT INTO PS_ISA_BILL_TRAN" & vbCrLf & _
                    " (BUSINESS_UNIT, INVOICE, LINE_SEQ_NUM, INVOICE_LINE," & vbCrLf & _
                    " BILL_TO_CUST_ID, SHIP_DATE, ISA_CUST_CHARGE_CD," & vbCrLf & _
                    " ISA_WORK_ORDER_NO, ISA_MACHINE_NO, ISA_EMPLOYEE_ID," & vbCrLf & _
                    " NET_EXTENDED_AMT, ORDER_NO, PO_REF, DESCR," & vbCrLf & _
                    " GROSS_EXTENDED_AMT, BATCH_NUMBER, PRODUCT_ID," & vbCrLf & _
                    " QTY, UNIT_OF_MEASURE," & vbCrLf & _
                    " ADD_DTTM, LAST_DTTIME)" & vbCrLf & _
                    " VALUES (" & vbCrLf & _
                    " '" & dtrow.Item("BUSINESS_UNIT") & "'," & vbCrLf & _
                    " '" & dtrow.Item("INVOICE") & "'," & vbCrLf & _
                    " " & dtrow.Item("LINE_SEQ_NUM") & "," & vbCrLf & _
                    " " & dtrow.Item("INVOICE_LINE") & "," & vbCrLf & _
                    " '" & dtrow.Item("BILL_TO_CUST_ID") & "'," & vbCrLf & _
                    " TO_DATE('" & dtrow.Item("SHIP_DT") & "', 'YYYY-MM-DD')," & vbCrLf & _
                    " '" & custChargeCode & "'," & vbCrLf & _
                    " '" & custWorkOrderNo & "'," & vbCrLf & _
                    " '" & Replace(dtrow.Item("ISA_MACHINE_NO"), "'", "''") & "'," & vbCrLf & _
                    " '" & dtrow.Item("ISA_EMPLOYEE_ID") & "'," & vbCrLf & _
                    " '" & dtrow.Item("NET_EXTENDED_AMT") & "'," & vbCrLf & _
                    " '" & dtrow.Item("ORDER_NO") & "'," & vbCrLf & _
                    " '" & dtrow.Item("PO_REF") & "'," & vbCrLf & _
                    " '" & Replace(dtrow.Item("DESCR"), "'", "''") & "'," & vbCrLf & _
                    " '" & dtrow.Item("GROSS_EXTENDED_AMT") & "'," & vbCrLf & _
                    " '" & dtrow.Item("BATCH_NUMBER") & "'," & vbCrLf & _
                    " '" & dtrow.Item("PRODUCT_ID") & "'," & vbCrLf & _
                    " '" & dtrow.Item("QTY") & "'," & vbCrLf & _
                    " '" & dtrow.Item("UNIT_OF_MEASURE") & "'," & vbCrLf & _
                    " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')," & vbCrLf & _
                    " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'))"

        Dim rowsaffected As Integer = ORDBAccess.ExecNonQuery(strSQLstring, connectOR)
        If rowsaffected = 0 Then
            objStreamWriter.WriteLine("  Error inserting into the INSERT INTO ISA_BILL_TRAN")
            Return True
        End If

    End Function

    Private Sub sendFTPfiles()

        Dim ff As clsFTP
        ' Create an instance of the FTP Class.
        ff = New clsFTP

        ' Setup the appropriate properties.

        'ff.RemoteHost = "ftp.sdi.com"
        'ff.RemoteUser = "ftp.deluxe.delivery"
        'ff.RemotePassword = "89tAXd#^"

        ff.RemoteHost = "lenka"
        ff.RemoteUser = "ftp.supplypro"
        ff.RemotePassword = "Tv06wsi0"

        Dim bContinue As Boolean = False
        Dim arrFiles As FileInfo() = (New DirectoryInfo(rootDir & "\XMLOUT\")).GetFiles("updUofPBil*.XML")
        If Not (arrFiles Is Nothing) Then
            If arrFiles.Length > 0 Then
                bContinue = True
            End If
        End If
        If bContinue Then
            If (ff.Login()) Then
                Try
                    ff.ChangeDirectory("XMLOut")
                    ff.SetBinaryMode(True)
                    ' get file list
                    Dim dirInfo As DirectoryInfo = New DirectoryInfo(rootDir & "\XMLOUT\")
                    Dim I As Integer
                    Dim strFiles As String
                    strFiles = "updUofPBil*.XML"
                    Dim aFiles As FileInfo() = dirInfo.GetFiles(strFiles)
                    For I = 0 To aFiles.Length - 1
                        'copy files to XMLIN directory

                        ff.UploadFile(aFiles(I).FullName)
                        objStreamWriter.WriteLine("     " & aFiles(I).FullName & " has been uploaded to the FTP site.")
                        Try
                            File.Move(aFiles(I).FullName, rootDir & "\XMLOUTProcessed\" & aFiles(I).Name)
                            File.Delete(aFiles(I).FullName)
                            objStreamWriter.WriteLine(" End - File " & aFiles(I).Name & " has been moved")
                        Catch ex As Exception
                            objStreamWriter.WriteLine("     Error " & ex.Message.ToString & " in file " & aFiles(I).Name)
                        End Try
                    Next

                Catch ex As System.Exception
                    objStreamWriter.WriteLine(" ERROR - uploading to the FTP site " & ex.ToString)
                Finally
                    Try
                        ff.CloseConnection()
                    Catch ex As Exception

                    End Try

                End Try
            Else
                objStreamWriter.WriteLine(" ERROR - no connection to FTP site")
            End If
        End If

    End Sub

    Private Sub SendEmail(ByVal bolError As Boolean)

        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "bob.dougherty@sdi.com"

        'The subject of the email
        If bolError = True Then
            email.Subject = "UofP Bil XML OUT Error"
        Else
            email.Subject = "UofP Bil has completed"
        End If

        'The Priority attached and displayed for the email
        email.Priority = MailPriority.High

        email.BodyFormat = MailFormat.Html

        If bolError = True Then
            email.Body = "<html><body><table><tr><td>UofP Bil has completed with errors, review log.</td></tr>"
        Else
            email.Body = "<html><body><table><tr><td>UofP Bil has completed.</td></tr>"
        End If

        'email.Body = email.Body & "<tr><td></td><a href='\\BDougherty_XP-l\logs'>\\BDougherty_XP-l\logs\</a></tr></table></body></html>"

        'Send the email and handle any error that occurs
        Try
            SmtpMail.Send(email)
        Catch
            objStreamWriter.WriteLine("     Error - the email was not sent")
        End Try

    End Sub

End Module
