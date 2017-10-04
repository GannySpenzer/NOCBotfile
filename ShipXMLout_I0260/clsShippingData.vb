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

        strSQLstring = "" & _
                       "SELECT " & vbCrLf & _
                       " PICK.ORDER_NO, PICK.SHIP_CNTR_ID, " & vbCrLf & _
                       " PICK.SHIP_DTTM, " & vbCrLf & _
                       " D.SHIPTO_ID, D.ISA_EMPLOYEE_ID AS EMPLID, D.ISA_WORK_ORDER_NO " & vbCrLf & _
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
                       " PS_ISA_ORD_INTF_HD C " & vbCrLf & _
                       ",PS_ISA_ORD_INTF_LN D " & vbCrLf & _
                       "WHERE PICK.ORDER_NO = C.ORDER_NO " & vbCrLf & _
                       "  AND PICK.ORDER_INT_LINE_NO = D.ISA_INTFC_LN " & vbCrLf & _
                       "  AND C.BUSINESS_UNIT_OM = D.BUSINESS_UNIT_OM " & vbCrLf & _
                       "  AND C.ORDER_NO = D.ORDER_NO " & vbCrLf & _
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
