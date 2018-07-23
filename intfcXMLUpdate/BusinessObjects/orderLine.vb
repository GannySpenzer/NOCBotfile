Public Class orderLine

    Implements IDisposable

    Public Enum eTargetOperation As Integer
        UnknownOperation = 0
        AddOrderLine = 1
        CancelOrderLine = 2
    End Enum

    Private m_id As Integer = -1
    Private m_order As order = Nothing
    Private m_lineNo As Integer = -1
    Private m_targetOp As orderLine.eTargetOperation = eTargetOperation.UnknownOperation

    Public Sub New(ByVal ord As order)
        m_order = ord
    End Sub

    Public Sub New(ByVal ord As order, ByVal lineNo As Integer)
        m_order = ord
        m_lineNo = lineNo
    End Sub

    Public ReadOnly Property ParentOrder() As order
        Get
            Return m_order
        End Get
    End Property

    Public Property [Id]() As Integer
        Get
            Return m_id
        End Get
        Set(ByVal Value As Integer)
            m_id = Value
        End Set
    End Property

    Public Property OrderLineNo() As Integer
        Get
            Return m_lineNo
        End Get
        Set(ByVal Value As Integer)
            m_lineNo = Value
        End Set
    End Property

    Public ReadOnly Property OrderLineKey() As String
        Get
            '// this will also mean that "Order_SDI" + "LineNo" will be unique
            '//     since the OrderNo_Group <-> Order_SDI is 1 to many
            Return (m_order.OrderNo_Group & "." & m_lineNo.ToString)
        End Get
    End Property

    Private m_orderDate As String = ""

    Public Property OrderDate() As String
        Get
            Return m_orderDate
        End Get
        Set(ByVal Value As String)
            m_orderDate = Value
        End Set
    End Property

    Private m_loc As String = ""

    Public Property Location() As String
        Get
            Return m_loc
        End Get
        Set(ByVal Value As String)
            m_loc = Value
        End Set
    End Property

    Private m_bu As String = ""

    Public Property BusinessUnit() As String
        Get
            Return m_bu
        End Get
        Set(ByVal Value As String)
            m_bu = Value
        End Set
    End Property

    Private m_statId As String = ""

    Public Property OrderLineStatus() As String
        Get
            Return m_statId
        End Get
        Set(ByVal Value As String)
            m_statId = Value
        End Set
    End Property

    Private m_priorityCode As String = ""

    Public Property PriorityCode() As String
        Get
            Return m_priorityCode
        End Get
        Set(ByVal Value As String)
            m_priorityCode = Value
        End Set
    End Property

    Public Property TargetOperation() As orderLine.eTargetOperation
        Get
            Return m_targetOp
        End Get
        Set(ByVal Value As orderLine.eTargetOperation)
            m_targetOp = Value
        End Set
    End Property

    Private m_invItemId As String = ""

    Public Property InventoryItemId() As String
        Get
            Return m_invItemId
        End Get
        Set(ByVal Value As String)
            m_invItemId = Value
        End Set
    End Property

    Private m_invItemDesc As String = ""

    Public Property InventoryItemDescription() As String
        Get
            Return m_invItemDesc
        End Get
        Set(ByVal Value As String)
            m_invItemDesc = Value
        End Set
    End Property

    Private m_mfgId As String = ""

    Public Property ManufacturerId() As String
        Get
            Return m_mfgId
        End Get
        Set(ByVal Value As String)
            m_mfgId = Value
        End Set
    End Property

    Private m_mfgItemId As String = ""

    Public Property ManufacturerItemId() As String
        Get
            Return m_mfgItemId
        End Get
        Set(ByVal Value As String)
            m_mfgItemId = Value
        End Set
    End Property

    Private m_qty As Double = CDbl("0")

    Public Property Quantity() As Double
        Get
            Return m_qty
        End Get
        Set(ByVal Value As Double)
            m_qty = Value
        End Set
    End Property

    Private m_dueDate As String = ""

    Public Property DueDate() As String
        Get
            Return m_dueDate
        End Get
        Set(ByVal Value As String)
            m_dueDate = Value
        End Set
    End Property
    Private m_refNum As String = ""

    Public Property ReferenceNo() As String
        Get
            Return m_refNum
        End Get
        Set(ByVal Value As String)
            m_refNum = Value
        End Set
    End Property
    Private m_refNum_line As String = ""

    Public Property ReferenceNoLine() As String
        Get
            Return m_refNum_line
        End Get
        Set(ByVal Value As String)
            m_refNum_line = Value
        End Set
    End Property
    Private m_Vendor_id As String = ""

    Public Property Vendor_id() As String
        Get
            Return m_Vendor_id
        End Get
        Set(ByVal Value As String)
            m_Vendor_id = Value
        End Set
    End Property
    Private m_ITM_ID_VNDR As String = ""

    Public Property ITM_ID_VNDR() As String
        Get
            Return m_ITM_ID_VNDR
        End Get
        Set(ByVal Value As String)
            m_ITM_ID_VNDR = Value
        End Set
    End Property
    Private m_ITM_ID_VNDR_AUX As String = ""

    Public Property ITM_ID_VNDR_AUX() As String
        Get
            Return m_ITM_ID_VNDR_AUX
        End Get
        Set(ByVal Value As String)
            m_ITM_ID_VNDR_AUX = Value
        End Set
    End Property
    Private m_empId As String = ""

    Public Property EmployeeId() As String
        Get
            Return m_empId
        End Get
        Set(ByVal Value As String)
            If Value = " " Or Value = "" Then
                m_empId = Value
            Else
                'pfd
                m_empId = Value
                If m_empId.Length > 8 Then
                    m_empId = m_empId.Substring(0, 8)

                End If

            End If

        End Set
    End Property

    Private m_netUnitPrice As Double = CDbl("0")
    Public Property NetUnitPrice() As Double
        Get
            Return m_netUnitPrice
        End Get
        Set(ByVal Value As Double)
            m_netUnitPrice = Value
        End Set
    End Property
    Private m_netPOPRICE As Double = CDbl("0")
    Public Property NetPOPrice() As Double
        Get
            Return m_netPOPRICE
        End Get
        Set(ByVal Value As Double)
            m_netPOPRICE = Value
        End Set
    End Property
    Private m_extendedAmt As Double = CDbl("0")

    Public Property ExtendedAmount() As Double
        Get
            Return m_extendedAmt
        End Get
        Set(ByVal Value As Double)
            m_extendedAmt = Value
        End Set
    End Property

    Private m_workOrderNo As String = ""

    Public Property WorkOrderNo() As String
        Get
            Return m_workOrderNo
        End Get
        Set(ByVal Value As String)
            m_workOrderNo = Value
        End Set
    End Property

    Private m_shipToId As String = ""

    Public Property ShipToId() As String
        Get
            Return m_shipToId
        End Get
        Set(ByVal Value As String)
            m_shipToId = Value
        End Set
    End Property

    Private m_custChargeCode As String = ""

    Public Property CustomerChargeCode() As String
        Get
            Return m_custChargeCode
        End Get
        Set(ByVal Value As String)
            m_custChargeCode = Value
        End Set
    End Property

    Private m_acctCode As String = ""

    Public Property Accountcode() As String
        Get
            Return m_acctCode
        End Get
        Set(ByVal Value As String)
            m_acctCode = Value
        End Set
    End Property

    Private m_uom As String = ""

    Public Property UOM() As String
        Get
            Return m_uom
        End Get
        Set(ByVal Value As String)
            m_uom = Value
        End Set
    End Property

    Private m_cost As Double = CDbl("0")

    Public Property Cost() As Double
        Get
            Return m_cost
        End Get
        Set(ByVal Value As Double)
            m_cost = Value
        End Set
    End Property

    Public Shared Function GetTargetOperation(ByVal opId As String) As orderLine.eTargetOperation
        Dim op As orderLine.eTargetOperation = eTargetOperation.UnknownOperation
        If Not (opId Is Nothing) Then
            If opId.Trim.Length > 0 Then
                Select Case opId.Trim.ToUpper
                    Case "I"
                        op = eTargetOperation.AddOrderLine
                    Case "P"
                        op = eTargetOperation.AddOrderLine
                    Case "C"
                        op = eTargetOperation.CancelOrderLine
                End Select

            End If
        End If
        Return op
    End Function

#Region " IDisposable Implementation "

    Public Sub Dispose() Implements System.IDisposable.Dispose

    End Sub

#End Region

End Class
