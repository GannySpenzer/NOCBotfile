Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports System.Data
Imports System.Web.mail
Imports System.IO
Imports System.Net

Module Module1

    Dim objStreamWriter As StreamWriter
    Dim logpath As String = "LOGS\UpdOrderStatus" & Now.ToString(format:="yyyyMMddHHmmss") & ".txt"

    Dim m_cConnectionString As String = "Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=RPTG"
    'Private Const m_cConnectionString As String = "Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=rptg"

    Sub Main()
        Dim bProceed As Boolean = True
        Dim dte6mths As Date
        dte6mths = Now.AddMonths(-3).ToString
        Dim I As Integer
        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("Start UpdOrderStatus " & Now())

        Dim connectOR As OleDbConnection

        '   (1) connection string / db connection
        Dim cnString As String = ""
        Try
            cnString = My.Settings("oraCNString1").ToString.Trim
        Catch ex As Exception
            cnString = "Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=RPTG"
        End Try
        If (cnString.Length > 0) Then
            m_cConnectionString = cnString
            ' drop current connection and re-create
            Try
                connectOR.Dispose()
            Catch ex As Exception
            End Try
            connectOR = Nothing
        End If

        Try
            connectOR = New OleDbConnection(m_cConnectionString)
        Catch ex As Exception
            bProceed = False
            objStreamWriter.WriteLine("Error instantiating connectOR: m_cConnectionString=" & m_cConnectionString & " ERROR=" & ex.Message)
        End Try
        If bProceed Then
            Try
                connectOR.Open()
            Catch ex As Exception
                bProceed = False
                objStreamWriter.WriteLine("Error opening connectOR: m_cConnectionString=" & m_cConnectionString & " ERROR=" & ex.Message)
            End Try
        End If

        If bProceed Then
            Console.WriteLine("Start Update of order status codes")
            Console.WriteLine("")

            ' 2011.01.05 - exclude AEES
            ' 2014.09.05 - INCLUDE AEES
            ' AEES list : ,'I0913','I0914','I0915','I0916','I0917','I0918','I0919','I0920','I0921','I0922','I0923','I0924','I0925','I0926','I0940'
            '   this is to identify if this program is causing "re-sourced" REQ->PO for INTFC_L.ISA_ORDER_STATUS to get set to CANCEL (C)
            '   creating a separate program for AEES ... and hopefully transition the rest after.
            '   - erwin
            'Dim commandOR As New OleDbCommand("SELECT B.ISA_IDENTIFIER, A.BUSINESS_UNIT_OM," & vbCrLf & _
            '            " A.ORDER_NO, B.LINE_NBR, A.ADD_DTTM, A.ORIGIN, A.ORDER_STATUS," & vbCrLf & _
            '            " B.ISA_PARENT_IDENT, B.ISA_ORDER_STATUS, B.QTY_REQ, B.ITM_SETID," & vbCrLf & _
            '            " B.INV_ITEM_ID, C.INV_STOCK_TYPE" & vbCrLf & _
            '            " FROM PS_ISA_ORD_INTFC_H A, PS_ISA_ORD_INTFC_L B, PS_INV_ITEMS C" & vbCrLf & _
            '            " WHERE A.ORDER_STATUS NOT IN ('C','X')" & vbCrLf & _
            '            " AND A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
            '            " AND B.ADD_DTTM > TO_DATE('" & dte6mths & "','MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
            '            " AND  C.SETID(+)  = B.ITM_SETID" & vbCrLf & _
            '            " AND  C.INV_ITEM_ID(+)  = B.INV_ITEM_ID" & vbCrLf & _
            '            " AND ( C.EFFDT =" & vbCrLf & _
            '            "  (SELECT MAX(A_ED.EFFDT) FROM PS_INV_ITEMS A_ED" & vbCrLf & _
            '            " WHERE B.ITM_SETID = A_ED.SETID" & vbCrLf & _
            '            " AND B.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf & _
            '            " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
            '            " OR C.EFFDT IS NULL)", connectOR)
            Dim commandOR As New OleDbCommand("SELECT B.ISA_IDENTIFIER, A.BUSINESS_UNIT_OM," & vbCrLf & _
                        " A.ORDER_NO, B.LINE_NBR, A.ADD_DTTM, A.ORIGIN, A.ORDER_STATUS," & vbCrLf & _
                        " B.ISA_PARENT_IDENT, B.ISA_ORDER_STATUS, B.QTY_REQ, B.ITM_SETID," & vbCrLf & _
                        " B.INV_ITEM_ID, C.INV_STOCK_TYPE" & vbCrLf & _
                             " ,B.QTY_SHIPPED " & vbCrLf & _
                        " FROM PS_ISA_ORD_INTFC_H A, PS_ISA_ORD_INTFC_L B, PS_INV_ITEMS C" & vbCrLf & _
                        " WHERE A.ORDER_STATUS NOT IN ('C','X')" & vbCrLf & _
                        " AND A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
                        " AND A.BUSINESS_UNIT_OM NOT IN ('I0418','I0419','I0420','I0421','I0422','I0423','I0424') " & vbCrLf & _
                        " AND B.ADD_DTTM > TO_DATE('" & dte6mths & "','MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                        " AND  C.SETID(+)  = B.ITM_SETID" & vbCrLf & _
                        " AND  C.INV_ITEM_ID(+)  = B.INV_ITEM_ID" & vbCrLf & _
                        " AND ( C.EFFDT = (SELECT MAX(A_ED.EFFDT) FROM PS_INV_ITEMS A_ED " & vbCrLf & _
                        "                  WHERE B.ITM_SETID = A_ED.SETID" & vbCrLf & _
                        "                    AND B.INV_ITEM_ID = A_ED.INV_ITEM_ID" & vbCrLf & _
                        "                    AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                        "       OR C.EFFDT IS NULL)", connectOR)

            ' " AND A.ORIGIN IN ('IOL','MOB','REQ')" & vbCrLf & _

            '" AND NOT B.ISA_ORDER_STATUS = '6'" & vbCrLf & _

            '" AND A.ORDER_NO = '4503176241'" & vbCrLf & _

            '" AND A.BUSINESS_UNIT_OM LIKE 'I03%'" & vbCrLf & _


            Console.WriteLine("  Start loading INTFC table data " & Now())
            objStreamWriter.WriteLine("  Start loading INTFC table data " & Now())
            objStreamWriter.WriteLine(Now() & vbCrLf & commandOR.CommandText)

            Dim dataAdapter As New OleDbDataAdapter(commandOR)
            Dim ds As New DataSet
            Try
                dataAdapter.Fill(ds)

            Catch OleDBExp As OleDbException
                Console.WriteLine("")
                Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                Console.WriteLine("")
                objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
            End Try


            Console.WriteLine("  Completed loading  INTFC table data " & Now())
            Console.WriteLine("")
            Dim bStatus As Boolean = False
            For I = 0 To ds.Tables(0).Rows.Count - 1
                ' if there is a re-quote - a line is added to a completed wo - then we need not update the header to 'O'
                ' we need to leave it at 'Q' and the line '2'
                ' pfd 10282008
                ' only if it is uncc
                If ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS") = "2" And ds.Tables(0).Rows(I).Item("ORDER_STATUS") = "Q" And ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM") = "I0256" Then
                    ' the header record will be "Q" if there is only one line with a "2"  we don't want to reset bstatus until the array  
                    ' loop is done.
                    bStatus = True
                End If
                If ds.Tables(0).Rows(I).Item("ISA_ORDER_STATUS") = "6" Then
                    If Not ds.Tables(0).Rows(I).Item("ORDER_STATUS") = "O" Then
                        Dim connectORUpd As New OleDbConnection(m_cConnectionString)
                        connectORUpd.Open()
                        If bStatus Then
                            objStreamWriter.WriteLine("          " & ds.Tables(0).Rows(I).Item("ORDER_NO") & _
                                " HDR was " & ds.Tables(0).Rows(I).Item("ORDER_STATUS") & _
                                " and no longer changing to Q")
                        Else
                            ' Change the current order header to "O" except if the current BU is
                            ' Ascend with order status = "Q". Ascend may add line-items to an
                            ' existing order so leave the "Q" even if any items were shipped (line-item status of "6").
                            If IsAscend(ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM")) And ds.Tables(0).Rows(I).Item("ORDER_STATUS") = "Q" Then
                                ' Log the condition that we're not changing anymore.
                                objStreamWriter.WriteLine("          " & ds.Tables(0).Rows(I).Item("ORDER_NO") & _
                                " HDR is " & ds.Tables(0).Rows(I).Item("ORDER_STATUS") & _
                                " but will not change to O since BU is " & ds.Tables(0).Rows(I).Item("BUSINESS_UNIT_OM"))
                            Else
                                objStreamWriter.WriteLine("          " & ds.Tables(0).Rows(I).Item("ORDER_NO") & _
                                    " HDR was " & ds.Tables(0).Rows(I).Item("ORDER_STATUS") & _
                                    " now O")
                                Dim commandORupd3H As New OleDbCommand("UPDATE PS_ISA_ORD_INTFC_H" & vbCrLf & _
                                    " SET ORDER_STATUS = 'O'" & vbCrLf & _
                                    " , LASTUPDDTTM = SYSDATE " & vbCrLf & _
                                    " WHERE ISA_IDENTIFIER = " & ds.Tables(0).Rows(I).Item("ISA_PARENT_IDENT") & "", connectORUpd)
                                Try
                                    commandORupd3H.ExecuteNonQuery()
                                    connectORUpd.Close()

                                Catch OleDBExp As OleDbException
                                    Console.WriteLine("")
                                    Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                                    Console.WriteLine("")
                                    objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
                                End Try
                            End If
                        End If
                    End If
                Else
                    Dim strStockType As String
                    If IsDBNull(ds.Tables(0).Rows(I).Item("INV_STOCK_TYPE")) Then
                        strStockType = "NSTK"
                    Else
                        strStockType = ds.Tables(0).Rows(I).Item("INV_STOCK_TYPE")
                    End If
                    If strStockType = "STK" Then
                        checkSTKstatus(ds.Tables(0).Rows(I))
                    Else
                        checkNSTKstatus(ds.Tables(0).Rows(I))
                    End If
                End If
                objStreamWriter.Flush()
            Next
        End If
        objStreamWriter.WriteLine("Complete UpdOrderStatus " & Now())
        Try
            connectOR.Close()
        Catch ex As Exception
            objStreamWriter.WriteLine("Error closing connectOR: m_cConnectionString=" & m_cConnectionString & " ERROR=" & ex.Message)
        End Try
        objStreamWriter.Close()
    End Sub

    Sub checkNSTKstatus(ByVal dsRow As DataRow)

        Dim bolNeedsUpdate As Boolean
        Dim bolNeedsHDRUpdate As Boolean
        Dim strLnStatus As String = " "
        Dim strReqStatus As String
        Dim strDistribLnStatus As String
        Dim strPOHdrStatus As String
        Dim decQtyLnRecvd As Decimal
        Dim decQtyLnRejct As Decimal
        Dim decQtyPO As Decimal
        Dim strPOID As String = " "
        Dim connectOR1 As New OleDbConnection(m_cConnectionString)
        Dim connectORUpd As New OleDbConnection(m_cConnectionString)
        connectOR1.Open()

        Dim commandOR1 As New OleDbCommand("SELECT A.REQ_ID, B.LINE_NBR, A.REQ_STATUS," & vbCrLf & _
                    " B.QTY_REQ," & vbCrLf & _
                    " B.INV_ITEM_ID" & vbCrLf & _
                    " FROM PS_REQ_HDR A, PS_REQ_LINE B" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT in ('ISA00','KEL00','SDC00','SDM00')" & vbCrLf & _
                    " AND A.REQ_ID = '" & dsRow.Item("ORDER_NO") & "'" & vbCrLf & _
                    " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
                    " AND A.REQ_ID = B.REQ_ID" & vbCrLf & _
                    " AND '" & dsRow.Item("LINE_NBR") & "' = B.LINE_NBR", connectOR1)

        Dim dr1 As OleDbDataReader
        Try
            dr1 = commandOR1.ExecuteReader

        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
        End Try

        If dr1.HasRows Then
            While dr1.Read
                bolNeedsUpdate = False
                If IsDBNull(dr1.Item("REQ_STATUS")) Then
                    strReqStatus = " "
                Else
                    strReqStatus = dr1.Item("REQ_STATUS")
                End If
                If strReqStatus = "X" And Not dsRow.Item("ISA_ORDER_STATUS") = "C" Then
                    bolNeedsUpdate = True
                    strLnStatus = "C"
                    'ElseIf strReqStatus = "A" And dsRow.Item("ISA_ORDER_STATUS") = "C" Then
                    '    bolNeedsUpdate = True
                    '    strLnStatus = "2"
                End If
                If bolNeedsUpdate = True Then
                    If dsRow.Item("ISA_ORDER_STATUS") = strLnStatus Then
                        bolNeedsUpdate = False
                    End If
                End If
                If bolNeedsUpdate = True Then
                    objStreamWriter.WriteLine("    NSTK Order - " & dr1.Item("REQ_ID") & _
                    " Line Nbr - " & dr1.Item("Line_Nbr") & _
                    " was " & dsRow.Item("ISA_ORDER_STATUS") & _
                    " now " & strLnStatus & " No PO")
                    connectORUpd.Open()
                    Dim commandORupd1 As New OleDbCommand("UPDATE PS_ISA_ORD_INTFC_L" & vbCrLf & _
                        " SET ISA_ORDER_STATUS = '" & strLnStatus & "'" & vbCrLf & _
                        " , LASTUPDDTTM = SYSDATE " & vbCrLf & _
                        " WHERE ISA_IDENTIFIER = " & dsRow.Item("ISA_IDENTIFIER") & vbCrLf & _
                        " AND ISA_PARENT_IDENT = " & dsRow.Item("ISA_PARENT_IDENT") & "", connectORUpd)
                    Try
                        commandORupd1.ExecuteNonQuery()

                    Catch OleDBExp As OleDbException
                        Console.WriteLine("")
                        Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                        Console.WriteLine("")
                        objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
                    End Try
                    connectORUpd.Close()
                End If

            End While
        Else
            bolNeedsUpdate = False
            If dsRow.Item("ORDER_STATUS") = "O" And Not dsRow.Item("ISA_ORDER_STATUS") = "C" Then
                ' We no longer need to change line items to cancel under this scenario.
                ' PeopleSoft is handling this scenario.
                'bolNeedsUpdate = True
                'strLnStatus = "C"
            End If
            If bolNeedsUpdate = True Then
                If dsRow.Item("ISA_ORDER_STATUS") = strLnStatus Then
                    bolNeedsUpdate = False
                End If
            End If
            If bolNeedsUpdate = True Then
                objStreamWriter.WriteLine("    NSTK Order - " & dsRow.Item("ORDER_NO") & _
                " Line Nbr - " & dsRow.Item("LINE_NBR") & _
                " was " & dsRow.Item("ISA_ORDER_STATUS") & _
                " now " & strLnStatus & " No Req")
                connectORUpd.Open()
                Dim commandORupd1 As New OleDbCommand("UPDATE PS_ISA_ORD_INTFC_L" & vbCrLf & _
                    " SET ISA_ORDER_STATUS = '" & strLnStatus & "'" & vbCrLf & _
                    " WHERE ISA_IDENTIFIER = " & dsRow.Item("ISA_IDENTIFIER") & vbCrLf & _
                    " AND ISA_PARENT_IDENT = " & dsRow.Item("ISA_PARENT_IDENT") & "", connectORUpd)
                Try
                    commandORupd1.ExecuteNonQuery()

                Catch OleDBExp As OleDbException
                    Console.WriteLine("")
                    Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                    Console.WriteLine("")
                    objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
                End Try
                connectORUpd.Close()
            End If
        End If

        connectORUpd.Open()
        dr1.Close()

        Dim commandOR1A As New OleDbCommand("SELECT A.REQ_ID, A.REQ_STATUS" & vbCrLf & _
                    " FROM PS_REQ_HDR A" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT in ('ISA00','KEL00','SDC00','SDM00')" & vbCrLf & _
                    " AND A.REQ_ID = '" & dsRow.Item("ORDER_NO") & "'" & vbCrLf & _
                    " AND NOT EXISTS (SELECT B.LINE_NBR" & vbCrLf & _
                    " FROM PS_REQ_LINE B" & vbCrLf & _
                    " WHERE B.LINE_NBR = '" & dsRow.Item("LINE_NBR") & "'" & vbCrLf & _
                    " AND B.REQ_ID = A.REQ_ID " & vbCrLf & _
                    " AND B.BUSINESS_UNIT = A.BUSINESS_UNIT ) ", connectOR1)
        ' " AND B.REQ_ID = A.REQ_ID) ", connectOR1)

        Dim dr1A As OleDbDataReader
        Try
            dr1A = commandOR1A.ExecuteReader

        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
        End Try


        While dr1A.Read
            bolNeedsUpdate = False
            If IsDBNull(dr1A.Item("REQ_STATUS")) Then
                strReqStatus = " "
            Else
                strReqStatus = dr1A.Item("REQ_STATUS")
            End If
            If strReqStatus = "X" And Not dsRow.Item("ISA_ORDER_STATUS") = "C" Then
                bolNeedsUpdate = True
                strLnStatus = "C"
            ElseIf strReqStatus = "C" Then
                bolNeedsUpdate = True
                strLnStatus = "6"
            ElseIf strReqStatus = "A" And _
                dsRow.Item("ADD_DTTM") < DateTime.Now.AddDays(-30) Then
                bolNeedsUpdate = True
                strLnStatus = "C"
            End If
            If bolNeedsUpdate = True Then
                If dsRow.Item("ISA_ORDER_STATUS") = strLnStatus Then
                    bolNeedsUpdate = False
                End If
            End If
            If bolNeedsUpdate = True Then
                objStreamWriter.WriteLine("    NSTK Order - " & dr1A.Item("REQ_ID") & _
                " Line Nbr - " & dsRow.Item("LINE_NBR") & _
                " was " & dsRow.Item("ISA_ORDER_STATUS") & _
                " now " & strLnStatus & " No Req Line")
                Dim commandORupd1A As New OleDbCommand("UPDATE PS_ISA_ORD_INTFC_L" & vbCrLf & _
                    " SET ISA_ORDER_STATUS = '" & strLnStatus & "'" & vbCrLf & _
                    " WHERE ISA_IDENTIFIER = " & dsRow.Item("ISA_IDENTIFIER") & vbCrLf & _
                    " AND ISA_PARENT_IDENT = " & dsRow.Item("ISA_PARENT_IDENT") & "", connectORUpd)
                Try
                    commandORupd1A.ExecuteNonQuery()

                Catch OleDBExp As OleDbException
                    Console.WriteLine("")
                    Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                    Console.WriteLine("")
                    objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
                End Try
            End If

        End While
        dr1A.Close()
        connectORUpd.Close()

        'Dim commandOR2 As New OleDbCommand("SELECT A.REQ_ID, B.LINE_NBR, A.REQ_STATUS," & vbCrLf & _
        '            " B.QTY_REQ, C.QTY_PO, C.PO_ID, C.LINE_NBR," & vbCrLf & _
        '            " B.INV_ITEM_ID, C.DISTRIB_LN_STATUS, D.PO_STATUS" & vbCrLf & _
        '            " FROM PS_REQ_HDR A, PS_REQ_LINE B," & vbCrLf & _
        '            " PS_PO_LINE_DISTRIB C, PS_PO_HDR D" & vbCrLf & _
        '            " WHERE A.BUSINESS_UNIT in ('ISA00','KEL00','SDC00','SDM00')" & vbCrLf & _
        '            " AND A.REQ_ID = '" & dsRow.Item("ORDER_NO") & "'" & vbCrLf & _
        '            " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
        '            " AND A.REQ_ID = B.REQ_ID" & vbCrLf & _
        '            " AND '" & dsRow.Item("LINE_NBR") & "' = B.LINE_NBR" & vbCrLf & _
        '            " AND B.BUSINESS_UNIT = C.BUSINESS_UNIT" & vbCrLf & _
        '            " AND B.REQ_ID = C.REQ_ID" & vbCrLf & _
        '            " AND B.LINE_NBR = C.REQ_LINE_NBR" & vbCrLf & _
        '            " AND C.BUSINESS_UNIT = D.BUSINESS_UNIT" & vbCrLf & _
        '            " AND C.PO_ID = D.PO_ID" & vbCrLf & _
        '            " ORDER BY C.PO_ID", connectOR1)
        '*****see below******pfd - 030032011
        ' filter out the the cancelled lines if the resourced active PO_ID  is greater than the PO_ID of the cancelled line
        Dim commandOR2 As New OleDbCommand("SELECT A.REQ_ID, B.LINE_NBR, A.REQ_STATUS," & vbCrLf & _
                    " B.QTY_REQ, C.QTY_PO, C.PO_ID, C.LINE_NBR," & vbCrLf & _
                    " B.INV_ITEM_ID, C.DISTRIB_LN_STATUS, D.PO_STATUS" & vbCrLf & _
                    " FROM PS_REQ_HDR A, PS_REQ_LINE B," & vbCrLf & _
                    " PS_PO_LINE_DISTRIB C, PS_PO_HDR D" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT in ('ISA00','KEL00','SDC00','SDM00')" & vbCrLf & _
                    " AND A.REQ_ID = '" & dsRow.Item("ORDER_NO") & "'" & vbCrLf & _
                    " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
                    " AND A.REQ_ID = B.REQ_ID" & vbCrLf & _
                    " AND '" & dsRow.Item("LINE_NBR") & "' = B.LINE_NBR" & vbCrLf & _
                    " AND B.BUSINESS_UNIT = C.BUSINESS_UNIT" & vbCrLf & _
                    " AND B.REQ_ID = C.REQ_ID" & vbCrLf & _
                    " AND B.LINE_NBR = C.REQ_LINE_NBR" & vbCrLf & _
                    " AND C.BUSINESS_UNIT = D.BUSINESS_UNIT" & vbCrLf & _
                    " AND C.PO_ID = D.PO_ID" & vbCrLf & _
                    " and not exists(select 'X' from   PS_REQ_HDR AA, PS_REQ_LINE BB," & vbCrLf & _
                    " PS_PO_LINE_DISTRIB CC, PS_PO_HDR DD where C.PO_ID < CC.PO_ID " & vbCrLf & _
                    " and CC.DISTRIB_LN_STATUS ='O' and C.DISTRIB_LN_STATUS ='X'" & vbCrLf & _
                    " and A.BUSINESS_UNIT in ('ISA00','KEL00','SDC00','SDM00') " & vbCrLf & _
                    " AND AA.REQ_ID = '" & dsRow.Item("ORDER_NO") & "'" & vbCrLf & _
                    " AND AA.BUSINESS_UNIT = BB.BUSINESS_UNIT " & vbCrLf & _
                    " AND AA.REQ_ID = BB.REQ_ID" & vbCrLf & _
                    " AND '" & dsRow.Item("LINE_NBR") & "' = BB.LINE_NBR " & vbCrLf & _
                    " AND BB.BUSINESS_UNIT = CC.BUSINESS_UNIT " & vbCrLf & _
                    " AND BB.REQ_ID = CC.REQ_ID " & vbCrLf & _
                    " AND BB.LINE_NBR = CC.REQ_LINE_NBR " & vbCrLf & _
                    " AND CC.BUSINESS_UNIT = DD.BUSINESS_UNIT " & vbCrLf & _
                    " AND CC.PO_ID = DD.PO_ID)" & vbCrLf & _
                    " ORDER BY C.PO_ID", connectOR1)
        'WITTMS1
        Dim dr2 As OleDbDataReader
        Try
            dr2 = commandOR2.ExecuteReader

        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
        End Try

        Dim bolPOCanceled As Boolean = False
        While dr2.Read
            bolNeedsUpdate = False
            bolNeedsHDRUpdate = False
            If IsDBNull(dr2.Item("DISTRIB_LN_STATUS")) Then
                strDistribLnStatus = " "
                strPOHdrStatus = " "
            Else
                strDistribLnStatus = dr2.Item("DISTRIB_LN_STATUS")
                strPOHdrStatus = dr2.Item("PO_STATUS")
            End If
            Dim reqStatus As String = ""
            Try
                reqStatus = CStr(dr2.Item("REQ_STATUS"))
            Catch ex As Exception
            End Try
            ' when PO was cancelled, it does not mean that order should be cancelled
            '   since buyer might be resourcing the REQ ... unless REQ was cancelled as well
            'If strDistribLnStatus = "X" And Not dsRow.Item("ISA_ORDER_STATUS") = "C" Then
            '    bolNeedsUpdate = True
            '    bolPOCanceled = True
            '    strLnStatus = "C"
            'ElseIf strDistribLnStatus = "O" And _
            If strDistribLnStatus = "X" And _
               Not dsRow.Item("ISA_ORDER_STATUS") = "C" And _
               reqStatus = "X" Then
                bolNeedsUpdate = True
                bolPOCanceled = True
                strLnStatus = "C"
            ElseIf strDistribLnStatus = "O" And _
                strPOHdrStatus = "D" And _
                dsRow.Item("ISA_ORDER_STATUS") = "2" Then
                bolNeedsUpdate = True
                strLnStatus = "3"
            ElseIf (strDistribLnStatus = "D" Or strDistribLnStatus = "O") And bolPOCanceled = True Then
                bolNeedsUpdate = True
                strLnStatus = "3"
                'pfd - 030032011 - check to see if this PO has been resourced...  where the interface line record is C and the req_line record is ok...
                'check the PS_PO_LINE_DISTRIB to find out if it has a deleted line and new line with a new PO_ID.... check the req_ID in the PS_PO_LINE_DISTRIB
                'filtered out the cancelled line if there is a PO_LINE_DISTRIB record with a PO_ID greater than the cancelled line. *****see above query*****

                'ElseIf (strDistribLnStatus = "D" Or strDistribLnStatus = "O") And dsRow.Item("ISA_ORDER_STATUS") = "C" Then
                '    bolNeedsUpdate = True
                '    strLnStatus = "3"
            ElseIf strDistribLnStatus = "O" And Not dsRow.Item("ORDER_STATUS") = "O" Then
                ' Change the current line-item status to "2" (processing order) 
                ' except if the current BU is Ascend with line-item status = "3" (ordered).
                ' Ascend may add line-items to an existing order so leave the "3" even if
                ' distribution line status is "O" (open).
                If Not (IsAscend(dsRow.Item("BUSINESS_UNIT_OM")) And dsRow.Item("ISA_ORDER_STATUS") = "3") Then
                    bolNeedsUpdate = True
                    bolNeedsHDRUpdate = True
                    strLnStatus = "2"
                End If
            End If
            If bolNeedsUpdate = True Then
                If dsRow.Item("ISA_ORDER_STATUS") = strLnStatus Then
                    bolNeedsUpdate = False
                End If
            End If
            If bolNeedsUpdate = True Then
                objStreamWriter.WriteLine("    NSTK Order - " & dr2.Item("REQ_ID") & _
                " Line Nbr - " & dr2.Item("Line_Nbr") & _
                " was " & dsRow.Item("ISA_ORDER_STATUS") & _
                " now " & strLnStatus & " No Recver")
                connectORUpd.Open()
                Dim commandORupd2 As New OleDbCommand("UPDATE PS_ISA_ORD_INTFC_L" & vbCrLf & _
                    " SET ISA_ORDER_STATUS = '" & strLnStatus & "'" & vbCrLf & _
                    " WHERE ISA_IDENTIFIER = " & dsRow.Item("ISA_IDENTIFIER") & vbCrLf & _
                    " AND ISA_PARENT_IDENT = " & dsRow.Item("ISA_PARENT_IDENT") & "", connectORUpd)
                Try
                    commandORupd2.ExecuteNonQuery()

                Catch OleDBExp As OleDbException
                    Console.WriteLine("")
                    Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                    Console.WriteLine("")
                    objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
                End Try
                connectORUpd.Close()

                If strDistribLnStatus = "O" And bolNeedsHDRUpdate = True Then
                    If Not dsRow.Item("ORDER_STATUS") = "C" And Not dsRow.Item("ORDER_STATUS") = "O" Then
                        objStreamWriter.WriteLine("    NSTK Order - " & dr2.Item("REQ_ID") & _
                        " HDR was " & dsRow.Item("ORDER_STATUS") & _
                        " now O")
                        connectORUpd.Open()
                        Dim commandORupd3H As New OleDbCommand("UPDATE PS_ISA_ORD_INTFC_H" & vbCrLf & _
                            " SET ORDER_STATUS = 'O'" & vbCrLf & _
                            " WHERE ISA_IDENTIFIER = " & dsRow.Item("ISA_PARENT_IDENT") & "", connectORUpd)

                        Try
                            commandORupd3H.ExecuteNonQuery()
                        Catch OleDBExp As OleDbException
                            Console.WriteLine("")
                            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                            Console.WriteLine("")
                            objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
                        End Try
                        connectORUpd.Close()
                    End If
                End If

            End If
            strPOID = dr2.Item("PO_ID")
        End While
        connectORUpd.Open()
        dr2.Close()

        '#If DEBUG Then
        '        If CStr(dsRow.Item("ORDER_NO")) = "A560000919" And _
        '           CInt(dsRow.Item("LINE_NBR")) = 1 Then
        '            Stop
        '        End If
        '#End If

        Dim commandOR3 As New OleDbCommand("" & _
                    "SELECT A.REQ_ID, B.LINE_NBR, A.REQ_STATUS," & vbCrLf & _
                    " B.QTY_REQ, C.QTY_PO, C.PO_ID, C.LINE_NBR," & vbCrLf & _
                    " B.INV_ITEM_ID, C.DISTRIB_LN_STATUS, D.QTY_LN_ACCPT," & vbCrLf & _
                    " D.QTY_LN_REJCT, D.RECV_LN_STATUS" & vbCrLf & _
                    " FROM PS_REQ_HDR A, PS_REQ_LINE B," & vbCrLf & _
                    " PS_PO_LINE_DISTRIB C," & vbCrLf & _
                    " ( SELECT  R.BUSINESS_UNIT, R.PO_ID, R.LINE_NBR, R.RECV_LN_STATUS," & vbCrLf & _
                    " SUM( R.QTY_LN_ACCPT) AS QTY_LN_ACCPT," & vbCrLf & _
                    " SUM( R.QTY_LN_REJCT) AS QTY_LN_REJCT" & vbCrLf & _
                    "   FROM 	PS_RECV_LN R" & vbCrLf & _
                    "   WHERE	R.BUSINESS_UNIT in ('ISA00','KEL00','SDC00','SDM00')" & vbCrLf & _
                    "   AND 	R.PO_ID = '" & strPOID & "'" & vbCrLf & _
                    "   GROUP BY R.BUSINESS_UNIT," & vbCrLf & _
                    "   R.PO_ID, R.LINE_NBR, R.RECV_LN_STATUS) D" & vbCrLf & _
                    " WHERE A.BUSINESS_UNIT in ('ISA00','KEL00','SDC00','SDM00')" & vbCrLf & _
                    " AND A.REQ_ID = '" & dsRow.Item("ORDER_NO") & "'" & vbCrLf & _
                    " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
                    " AND A.REQ_ID = B.REQ_ID" & vbCrLf & _
                    " AND '" & dsRow.Item("LINE_NBR") & "' = B.LINE_NBR" & vbCrLf & _
                    " AND B.BUSINESS_UNIT = C.BUSINESS_UNIT" & vbCrLf & _
                    " AND B.REQ_ID = C.REQ_ID" & vbCrLf & _
                    " AND B.LINE_NBR = C.REQ_LINE_NBR" & vbCrLf & _
                    " AND NOT C.DISTRIB_LN_STATUS = 'X'" & vbCrLf & _
                    " AND C.BUSINESS_UNIT = D.BUSINESS_UNIT" & vbCrLf & _
                    " AND C.PO_ID = D.PO_ID" & vbCrLf & _
                    " AND C.LINE_NBR = D.LINE_NBR", connectOR1)

        '" & vbCrLf & _
        '            "   GROUP BY A.BUSINESS_UNIT," & vbCrLf & _
        '            "   A.PO_ID, A.LINE_NBR," & vbCrLf & _
        '            "   A.SCHED_NBR," & vbCrLf & _
        '            "   A.DUE_DT, A.QTY_PO 
        Dim dr3 As OleDbDataReader
        Try
            dr3 = commandOR3.ExecuteReader

        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
        End Try

        While dr3.Read
            bolNeedsUpdate = False
            Dim nCurrentQtyShipped As Decimal = 0
            If Not IsDBNull(dsRow("QTY_SHIPPED")) Then
                Try
                    nCurrentQtyShipped = CDec(dsRow("QTY_SHIPPED"))
                Catch ex As Exception
                End Try
            End If
            If IsDBNull(dr3.Item("QTY_LN_ACCPT")) Then
                decQtyLnRecvd = 0
            Else
                decQtyLnRecvd = dr3.Item("QTY_LN_ACCPT")
            End If
            If IsDBNull(dr3.Item("QTY_LN_REJCT")) Then
                decQtyLnRejct = 0
            Else
                decQtyLnRejct = dr3.Item("QTY_LN_REJCT")
            End If
            If IsDBNull(dr3.Item("QTY_PO")) Then
                decQtyPO = 0
            Else
                decQtyPO = dr3.Item("QTY_PO")
            End If
            ' why would you cancel an order
            'If decQtyLnRejct > 0 And Not dsRow.Item("ISA_ORDER_STATUS") = "C" Then
            '    If decQtyPO = decQtyLnRejct Or _
            '        decQtyPO < decQtyLnRejct Then
            '        bolNeedsUpdate = True
            '        strLnStatus = "C"
            '    End If
            'End If
            If decQtyLnRecvd > 0 And Not strLnStatus = "C" And Not dsRow.Item("ISA_ORDER_STATUS") = "C" Then
                If decQtyLnRecvd = decQtyPO Or _
                    decQtyPO < decQtyLnRecvd Then
                    bolNeedsUpdate = True
                    strLnStatus = "6"
                Else
                    If Not dsRow.Item("ISA_ORDER_STATUS") = "5" Then
                        bolNeedsUpdate = True
                        strLnStatus = "5"
                    End If

                End If
            End If
            If bolNeedsUpdate = True Then
                If dsRow.Item("ISA_ORDER_STATUS") = strLnStatus Then
                    bolNeedsUpdate = False
                End If
            End If
            If bolNeedsUpdate = True Then
                objStreamWriter.WriteLine("    NSTK Order - " & dr3.Item("REQ_ID") & _
                " Line Nbr - " & dr3.Item("Line_Nbr") & _
                " was " & dsRow.Item("ISA_ORDER_STATUS") & _
                " now " & strLnStatus & " QtyPO - " & decQtyPO & _
                " Qty Recv - " & decQtyLnRecvd & " Qty Reject - " & decQtyLnRejct)
                Dim commandORupd3 As New OleDbCommand("UPDATE PS_ISA_ORD_INTFC_L" & vbCrLf & _
                    " SET ISA_ORDER_STATUS = '" & strLnStatus & "'," & vbCrLf & _
                    " QTY_SHIPPED = " & decQtyLnRecvd & vbCrLf & _
                    " WHERE ISA_IDENTIFIER = " & dsRow.Item("ISA_IDENTIFIER") & vbCrLf & _
                    " AND ISA_PARENT_IDENT = " & dsRow.Item("ISA_PARENT_IDENT") & "", connectORUpd)
                Try
                    commandORupd3.ExecuteNonQuery()

                Catch OleDBExp As OleDbException
                    Console.WriteLine("")
                    Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                    Console.WriteLine("")
                    objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
                End Try


                If Not strLnStatus = "C" Then
                    If Not dsRow.Item("ORDER_STATUS") = "C" And Not dsRow.Item("ORDER_STATUS") = "O" Then
                        objStreamWriter.WriteLine("          " & dr3.Item("REQ_ID") & _
                        " HDR was " & dsRow.Item("ORDER_STATUS") & _
                        " now O")
                        Dim commandORupd3H As New OleDbCommand("UPDATE PS_ISA_ORD_INTFC_H" & vbCrLf & _
                            " SET ORDER_STATUS = 'O'" & vbCrLf & _
                            " WHERE ISA_IDENTIFIER = " & dsRow.Item("ISA_PARENT_IDENT") & "", connectORUpd)

                        Try
                            commandORupd3H.ExecuteNonQuery()

                        Catch OleDBExp As OleDbException
                            Console.WriteLine("")
                            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                            Console.WriteLine("")
                            objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
                        End Try

                    End If
                End If
            End If
            ' Do update
        End While
        dr3.Close()
        connectOR1.Close()
        connectORUpd.Close()
    End Sub

    Sub checkSTKstatus(ByVal dsRow As DataRow)

        Dim bolNeedsUpdate As Boolean
        Dim strLnStatus As String = " "
        Dim I As Integer
        Dim decQtyShipped As Decimal
        Dim connectOR1 As New OleDbConnection(m_cConnectionString)
        Dim connectORUpd As New OleDbConnection(m_cConnectionString)
        connectOR1.Open()
        connectORUpd.Open()
        Dim commandOR1 As New OleDbCommand("SELECT  A.ORDER_NO, A.ORDER_INT_LINE_NO," & vbCrLf & _
                        " A.INV_ITEM_ID, SUM(A.QTY_SHIPPED) as QTY_SHIPPED," & vbCrLf & _
                        " A.CANCEL_FLAG, A.SHIPPED_FLAG" & vbCrLf & _
                        " FROM PS_ISA_ORD_INTFC_O B, PS_SHIP_INF_INV A" & vbCrLf & _
                        " WHERE B.ORDER_NO = '" & dsRow.Item("ORDER_NO") & "'" & vbCrLf & _
                        " AND B.ISA_IDENTIFIER = '" & dsRow.Item("ISA_IDENTIFIER") & "'" & vbCrLf & _
                        " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
                        " AND A.ORDER_INT_LINE_NO = B.ORDER_INT_LINE_NO" & vbCrLf & _
                        " AND A.INV_ITEM_ID = '" & dsRow.Item("INV_ITEM_ID") & "'" & vbCrLf & _
                        " AND A.DEMAND_SOURCE = 'OM'" & vbCrLf & _
                        " GROUP BY A.ORDER_NO, A.ORDER_INT_LINE_NO," & vbCrLf & _
                        " A.INV_ITEM_ID, A.CANCEL_FLAG, A.SHIPPED_FLAG" & vbCrLf & _
                        " ORDER BY A.CANCEL_FLAG", connectOR1)

        Dim dr1 As OleDbDataReader
        Try
            dr1 = commandOR1.ExecuteReader

        Catch OleDBExp As OleDbException
            Console.WriteLine("")
            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
            Console.WriteLine("")
            objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
        End Try

        I = 0
        decQtyShipped = 0
        While dr1.Read
            I = I + 1
            decQtyShipped = decQtyShipped + dr1.Item("QTY_SHIPPED")
            bolNeedsUpdate = False
            'If (dr1.Item("CANCEL_FLAG") = "Y") And _
            '    (decQtyShipped = 0) And _
            '    (Not dsRow.Item("ISA_ORDER_STATUS") = "C") Then
            '    bolNeedsUpdate = True
            '    strLnStatus = "C"
            'Elsef
            If dr1.Item("SHIPPED_FLAG") = "Y" And _
                dsRow.Item("QTY_REQ") > 0 And _
                Not dr1.Item("CANCEL_FLAG") = "Y" Then
                If dsRow.Item("QTY_REQ") = dr1.Item("QTY_SHIPPED") Or _
                    dsRow.Item("QTY_REQ") < dr1.Item("QTY_SHIPPED") Then
                    bolNeedsUpdate = True
                    strLnStatus = "6"
                Else
                    If dr1.Item("QTY_SHIPPED") > 0 Then
                        bolNeedsUpdate = True
                        strLnStatus = "5"
                        'ElseIf Not dsRow.Item("ISA_ORDER_STATUS") = "4" Then
                        '    bolNeedsUpdate = True
                        '    strLnStatus = "3"
                    End If
                End If
            End If
            If bolNeedsUpdate = True Then
                If dsRow.Item("ISA_ORDER_STATUS") = strLnStatus Then
                    bolNeedsUpdate = False
                End If
            End If

            If bolNeedsUpdate = True Then
                objStreamWriter.WriteLine("     STK Order - " & dr1.Item("ORDER_NO") & _
                " Line Nbr - " & dr1.Item("ORDER_INT_LINE_NO") & _
                " Item ID - " & dr1.Item("INV_ITEM_ID") & _
                " was " & dsRow.Item("ISA_ORDER_STATUS") & _
                " now " & strLnStatus & " QtyReq - " & dsRow.Item("QTY_REQ") & _
                " Qty Ship - " & dr1.Item("QTY_SHIPPED") & " Qty Cancelled - " & dr1.Item("CANCEL_FLAG"))
                Dim commandORupd1 As New OleDbCommand("UPDATE PS_ISA_ORD_INTFC_L" & vbCrLf & _
                    " SET ISA_ORDER_STATUS = '" & strLnStatus & "'," & vbCrLf & _
                    " QTY_SHIPPED = " & dr1.Item("QTY_SHIPPED") & vbCrLf & _
                    " WHERE ISA_IDENTIFIER = " & dsRow.Item("ISA_IDENTIFIER") & vbCrLf & _
                    " AND ISA_PARENT_IDENT = " & dsRow.Item("ISA_PARENT_IDENT") & "", connectORUpd)
                Try
                    commandORupd1.ExecuteNonQuery()

                Catch OleDBExp As OleDbException
                    Console.WriteLine("")
                    Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                    Console.WriteLine("")
                    objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
                End Try

                If Not strLnStatus = "C" Then
                    If Not dsRow.Item("ORDER_STATUS") = "C" And Not dsRow.Item("ORDER_STATUS") = "O" Then
                        objStreamWriter.WriteLine("          " & dr1.Item("ORDER_NO") & _
                        " HDR was " & dsRow.Item("ORDER_STATUS") & _
                        " now O")
                        Dim commandORupd1H As New OleDbCommand("UPDATE PS_ISA_ORD_INTFC_H" & vbCrLf & _
                            " SET ORDER_STATUS = 'O'" & vbCrLf & _
                            " WHERE ISA_IDENTIFIER = " & dsRow.Item("ISA_PARENT_IDENT") & "", connectORUpd)

                        Try
                            commandORupd1H.ExecuteNonQuery()

                        Catch OleDBExp As OleDbException
                            Console.WriteLine("")
                            Console.WriteLine("***OLEDB error - " & OleDBExp.ToString)
                            Console.WriteLine("")
                            objStreamWriter.WriteLine("     Error " & OleDBExp.ToString)
                        End Try

                    End If
                End If
            End If
            ' Do update
        End While
        dr1.Close()
        connectOR1.Close()
        connectORUpd.Close()
    End Sub

    Private Function IsAscend(sBU As String) As Boolean
        Const cAscendBUs As String = "~I0440~I0441~I0442~I0443~I0444"

        Dim bIsAscend As Boolean = False

        If cAscendBUs.IndexOf(sBU.Trim.ToUpper) >= 0 Then
            bIsAscend = True
        End If

        Return bIsAscend
    End Function

End Module
