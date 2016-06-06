Imports System.Data.OleDb
Public Class clsShippingData

    Private strOrderNo As String
    Public ReadOnly Property OrderNo() As String
        Get
            Return strOrderNo
        End Get
    End Property

    Private decQtyPicked As Decimal = 0
    Public ReadOnly Property QtyPicked() As String
        Get
            Return decQtyPicked
        End Get
    End Property

    Private strPackDTTM As String = ""
    Public ReadOnly Property PackDTTM() As String
        Get
            Return strPackDTTM
        End Get
    End Property

    Private strDescr As String
    Public ReadOnly Property Descr() As String
        Get
            Return strDescr
        End Get
    End Property

    Private strAddress1 As String
    Public ReadOnly Property Address1() As String
        Get
            Return strAddress1
        End Get
    End Property

    Private strAddress2 As String
    Public ReadOnly Property Address2() As String
        Get
            Return strAddress2
        End Get
    End Property

    Private strAddress3 As String
    Public ReadOnly Property Address3() As String
        Get
            Return strAddress3
        End Get
    End Property

    Private strAddress4 As String
    Public ReadOnly Property Address4() As String
        Get
            Return strAddress4
        End Get
    End Property

    Private strCity As String
    Public ReadOnly Property City() As String
        Get
            Return strCity
        End Get
    End Property

    Private strState As String
    Public ReadOnly Property State() As String
        Get
            Return strState
        End Get
    End Property

    Private strPostal As String
    Public ReadOnly Property Postal() As String
        Get
            Return strPostal
        End Get
    End Property

    Private strWorkOrderNo As String
    Public ReadOnly Property WorkOrderNo() As String
        Get
            Return strWorkOrderNo
        End Get
    End Property

    Private strEmplID As String
    Public ReadOnly Property EmplID() As String
        Get
            Return strEmplID
        End Get
    End Property

    Public Sub New(ByVal strContainerID As String, _
                   ByVal connector As OleDbConnection, _
                   Optional ByVal orderNo As String = "")
        Dim strSQLstring As String
        '' revert back to 08/07/2006 version of this SQL statement
        ''   which works. The change on 10/29/2007 is to getting an Oracle error
        '' 20080507 - erwin
        ''strSQLstring = "SELECT A.ORDER_NO, A.SHIP_CNTR_ID," & vbCrLf & _
        ''            " TO_CHAR(B.ISA_PACK_DTTM,'YYYY-MM-DD-HH24.MI.SS') as PACK_DTTM," & vbCrLf & _
        ''            " SUM(B.QTY_PICKED) as QTY_PICKED," & vbCrLf & _
        ''            " D.SHIPTO_ID, D.EMPLID, E.DESCR, E.ADDRESS1, E.ADDRESS2, E.ADDRESS3," & vbCrLf & _
        ''            " E.ADDRESS4, E.CITY, E.STATE, E.POSTAL, F.ISA_WORK_ORDER_NO" & vbCrLf & _
        ''            " FROM PS_ISA_PICKING_CNT A, PS_ISA_PICKING_INT B," & vbCrLf & _
        ''            " PS_ISA_ORD_INTFC_H C, PS_ISA_ORD_INTFC_L D," & vbCrLf & _
        ''            " PS_LOCATION_TBL E, PS_ORD_LINE F" & vbCrLf & _
        ''            " WHERE A.SHIP_CNTR_ID = '" & strContainerID & "'" & vbCrLf & _
        ''            " AND  ROWNUM = '1'" & vbCrLf & _
        ''            " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
        ''            " AND A.DEMAND_SOURCE = B.DEMAND_SOURCE" & vbCrLf & _
        ''            " AND A.SOURCE_BUS_UNIT = B.SOURCE_BUS_UNIT" & vbCrLf & _
        ''            " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
        ''            " AND A.ORDER_INT_LINE_NO = B.ORDER_INT_LINE_NO" & vbCrLf & _
        ''            " AND A.SCHED_LINE_NO = B.SCHED_LINE_NO" & vbCrLf & _
        ''            " AND A.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf & _
        ''            " AND A.DEMAND_LINE_NO = B.DEMAND_LINE_NO" & vbCrLf & _
        ''            " AND A.SEQ_NBR = B.SEQ_NBR" & vbCrLf & _
        ''            " AND A.RECEIVER_ID = B.RECEIVER_ID" & vbCrLf & _
        ''            " AND A.RECV_LN_NBR = B.RECV_LN_NBR" & vbCrLf & _
        ''            " AND A.ORDER_NO = C.ORDER_NO" & vbCrLf & _
        ''            " AND C.ISA_IDENTIFIER = D.ISA_PARENT_IDENT" & vbCrLf & _
        ''            " AND D.SHIPTO_ID = E.LOCATION" & vbCrLf & _
        ''            " AND E.EFFDT =" & vbCrLf & _
        ''            " (SELECT MAX(E_ED.EFFDT) FROM PS_LOCATION_TBL E_ED" & vbCrLf & _
        ''            " WHERE E.SETID = E_ED.SETID" & vbCrLf & _
        ''            " AND E.LOCATION = E_ED.LOCATION" & vbCrLf & _
        ''            " AND E_ED.EFFDT <= SYSDATE)" & vbCrLf & _
        ''            " AND E.EFF_STATUS = 'A'" & vbCrLf & _
        ''            " AND B.ORDER_NO = F.ORDER_NO" & vbCrLf & _
        ''            " AND B.ORDER_INT_LINE_NO = F.ORDER_INT_LINE_NO" & vbCrLf
        'strSQLstring = "SELECT A.ORDER_NO, A.SHIP_CNTR_ID," & vbCrLf & _
        '            " TO_CHAR(B.ISA_PACK_DTTM,'YYYY-MM-DD-HH24.MI.SS') as PACK_DTTM," & vbCrLf & _
        '            " D.SHIPTO_ID, D.EMPLID, E.DESCR, E.ADDRESS1, E.ADDRESS2, E.ADDRESS3," & vbCrLf & _
        '            " E.ADDRESS4, E.CITY, E.STATE, E.POSTAL, F.ISA_WORK_ORDER_NO" & vbCrLf & _
        '            " FROM PS_ISA_PICKING_CNT A, PS_ISA_PICKING_INT B," & vbCrLf & _
        '            " PS_ISA_ORD_INTFC_H C, PS_ISA_ORD_INTFC_L D," & vbCrLf & _
        '            " PS_LOCATION_TBL E, PS_ORD_LINE F" & vbCrLf & _
        '            " WHERE A.SHIP_CNTR_ID = '" & strContainerID & "'" & vbCrLf & _
        '            " AND  ROWNUM = '1'" & vbCrLf & _
        '            " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
        '            " AND A.DEMAND_SOURCE = B.DEMAND_SOURCE" & vbCrLf & _
        '            " AND A.SOURCE_BUS_UNIT = B.SOURCE_BUS_UNIT" & vbCrLf & _
        '            " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
        '            " AND A.ORDER_INT_LINE_NO = B.ORDER_INT_LINE_NO" & vbCrLf & _
        '            " AND A.SCHED_LINE_NO = B.SCHED_LINE_NO" & vbCrLf & _
        '            " AND A.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf & _
        '            " AND A.DEMAND_LINE_NO = B.DEMAND_LINE_NO" & vbCrLf & _
        '            " AND A.SEQ_NBR = B.SEQ_NBR" & vbCrLf & _
        '            " AND A.RECEIVER_ID = B.RECEIVER_ID" & vbCrLf & _
        '            " AND A.RECV_LN_NBR = B.RECV_LN_NBR" & vbCrLf & _
        '            " AND A.ORDER_NO = C.ORDER_NO" & vbCrLf & _
        '            " AND C.ISA_IDENTIFIER = D.ISA_PARENT_IDENT" & vbCrLf & _
        '            " AND D.SHIPTO_ID = E.LOCATION" & vbCrLf & _
        '            " AND E.EFFDT =" & vbCrLf & _
        '            " (SELECT MAX(E_ED.EFFDT) FROM PS_LOCATION_TBL E_ED" & vbCrLf & _
        '            " WHERE E.SETID = E_ED.SETID" & vbCrLf & _
        '            " AND E.LOCATION = E_ED.LOCATION" & vbCrLf & _
        '            " AND E_ED.EFFDT <= SYSDATE)" & vbCrLf & _
        '            " AND E.EFF_STATUS = 'A'" & vbCrLf & _
        '            " AND B.ORDER_NO = F.ORDER_NO" & vbCrLf & _
        '            " AND B.ORDER_INT_LINE_NO = F.ORDER_INT_LINE_NO" & vbCrLf
        strSQLstring = "" & _
                       "SELECT " & vbCrLf & _
                       " PICK.ORDER_NO, PICK.SHIP_CNTR_ID, " & vbCrLf & _
                       " PICK.SHIP_DTTM, " & vbCrLf & _
                       " D.SHIPTO_ID, D.EMPLID, D.ISA_WORK_ORDER_NO " & vbCrLf & _
                       "FROM " & vbCrLf & _
                       " ( " & vbCrLf & _
                       "   SELECT " & vbCrLf & _
                       "    INT.ORDER_NO " & vbCrLf & _
                       "   ,INT.ORDER_INT_LINE_NO " & vbCrLf & _
                       "   ,CNT.SHIP_CNTR_ID " & vbCrLf & _
                       "   ,MAX(INT.SHIP_DTTM) AS SHIP_DTTM " & vbCrLf & _
                       "   FROM " & vbCrLf & _
                       "    PS_ISA_PICKING_CNT CNT " & vbCrLf & _
                       "   ,PS_ISA_PICKING_INT INT " & vbCrLf & _
                       "   WHERE CNT.BUSINESS_UNIT = INT.BUSINESS_UNIT " & vbCrLf & _
                       "     AND CNT.DEMAND_SOURCE = INT.DEMAND_SOURCE " & vbCrLf & _
                       "     AND CNT.SOURCE_BUS_UNIT = INT.SOURCE_BUS_UNIT " & vbCrLf & _
                       "     AND CNT.ORDER_NO = INT.ORDER_NO " & vbCrLf & _
                       "     AND CNT.ORDER_INT_LINE_NO = INT.ORDER_INT_LINE_NO " & vbCrLf & _
                       "     AND CNT.SCHED_LINE_NO = INT.SCHED_LINE_NO " & vbCrLf & _
                       "     AND CNT.INV_ITEM_ID = INT.INV_ITEM_ID " & vbCrLf & _
                       "     AND CNT.DEMAND_LINE_NO = INT.DEMAND_LINE_NO " & vbCrLf & _
                       "     AND CNT.SEQ_NBR = INT.SEQ_NBR " & vbCrLf & _
                       "     AND CNT.RECEIVER_ID = INT.RECEIVER_ID " & vbCrLf & _
                       "     AND CNT.RECV_LN_NBR = INT.RECV_LN_NBR " & vbCrLf & _
                       "     AND CNT.SHIP_CNTR_ID = '" & strContainerID & "'" & vbCrLf & _
                       "     AND INT.SHIP_DTTM IS NOT NULL " & vbCrLf & _
                       "     AND CNT.BUSINESS_UNIT LIKE '%260' " & vbCrLf & _
                       "     AND CNT.DEMAND_SOURCE = 'OM' " & vbCrLf & _
                       "   GROUP BY " & vbCrLf & _
                       "    INT.ORDER_NO " & vbCrLf & _
                       "   ,INT.ORDER_INT_LINE_NO " & vbCrLf & _
                       "   ,CNT.SHIP_CNTR_ID " & vbCrLf & _
                       " ) PICK," & vbCrLf & _
                       " PS_ISA_ORD_INTFC_H C " & vbCrLf & _
                       ",PS_ISA_ORD_INTFC_L D " & vbCrLf & _
                       "WHERE PICK.ORDER_NO = C.ORDER_NO " & vbCrLf & _
                       "  AND PICK.ORDER_INT_LINE_NO = D.LINE_NBR " & vbCrLf & _
                       "  AND C.ISA_IDENTIFIER = D.ISA_PARENT_IDENT " & vbCrLf & _
                       ""
        If orderNo.Length > 0 Then
            strSQLstring &= "" & _
                       "  AND PICK.ORDER_NO = '" & orderNo & "'" & vbCrLf & _
                            ""
        End If

        Dim objReader As OleDbDataReader = ORDBAccess.GetReader(strSQLstring, connector)
        If objReader.Read() Then
            strOrderNo = objReader.Item("ORDER_NO")

            ' revert back to 08/07/2006 version of this SQL statement
            '   which works. The change on 10/29/2007 is to getting an Oracle error
            ' 20080507 - erwin
            'decQtyPicked = objReader.Item("QTY_PICKED")

            If IsDBNull(objReader.Item("SHIP_DTTM")) Then
                strPackDTTM = ""
            Else
                Try
                    strPackDTTM = CStr(objReader.Item("SHIP_DTTM"))
                Catch ex As Exception
                End Try
            End If
            strOrderNo = objReader.Item("ORDER_NO")
            strWorkOrderNo = objReader.Item("ISA_WORK_ORDER_NO")
            strEmplID = objReader.Item("EMPLID")

        End If
        connector.Close()
    End Sub

End Class
