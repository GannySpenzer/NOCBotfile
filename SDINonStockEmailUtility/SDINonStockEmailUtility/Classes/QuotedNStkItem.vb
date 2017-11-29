'
' Object class for holding Quoted Non-Stock item values
'
Public Class QuotedNStkItem

    Private m_sID As String = ""
    Private m_nIndex As Integer = -1

    Private m_sTO As String = ""
    Private m_sFROM As String = ""
    Private m_sCC As String = ""
    Private m_sBCC As String = ""
    Private m_sSubject As String = ""

    Private m_sOrderID As String = ""
    Private m_sFormattedOrderID As String = ""
    Private m_sBusinessUnitID As String = ""
    Private m_sBusinessUnitOM As String = ""
    Private m_sCustomerID As String = ""
    Private m_sEmployeeID As String = ""
    Private m_sAddressee As String = ""

    Private m_workOrderNo As String = ""
    Private m_orderOrigin As String = ""

    Private m_priceblock As String = ""
    Private m_LineStatus As String = ""

    ' VR 11/20/2014 Adding new members to display in E-mail
    Private m_Buyer_Id As String = ""
    Private m_Buyer_Email As String = ""
    '
    ' for backup recipient(s)
    '
    Private m_ApprovalLimit As Decimal = 0
    Private m_arrEmpIDs As New ArrayList


    Public Property ID() As String
        Get
            Return m_sID
        End Get
        Set(ByVal Value As String)
            m_sID = Value
        End Set
    End Property
    Public Property LineStatus() As String
        Get
            Return m_LineStatus
        End Get
        Set(value As String)
            m_LineStatus = value
        End Set
    End Property

    Friend Property IndexInCollection() As Integer
        Get
            Return m_nIndex
        End Get
        Set(ByVal Value As Integer)
            m_nIndex = Value
        End Set
    End Property

    Public Property [TO]() As String
        Get
            Return m_sTO
        End Get
        Set(ByVal Value As String)
            m_sTO = Value
        End Set
    End Property

    Public Property [FROM]() As String
        Get
            Return m_sFROM
        End Get
        Set(ByVal Value As String)
            m_sFROM = Value
        End Set
    End Property

    Public Property CC() As String
        Get
            Return m_sCC
        End Get
        Set(ByVal Value As String)
            m_sCC = Value
        End Set
    End Property

    Public Property BCC() As String
        Get
            Return m_sBCC
        End Get
        Set(ByVal Value As String)
            m_sBCC = Value
        End Set
    End Property

    Public Property Subject() As String
        Get
            Return m_sSubject
        End Get
        Set(ByVal Value As String)
            m_sSubject = Value
        End Set
    End Property

    Public Property OrderID() As String
        Get
            Return m_sOrderID
        End Get
        Set(ByVal Value As String)
            m_sOrderID = Value
        End Set
    End Property

    Public Property FormattedOrderID() As String
        Get
            Return m_sFormattedOrderID
        End Get
        Set(ByVal Value As String)
            m_sFormattedOrderID = Value
        End Set
    End Property

    Public Property BusinessUnitID() As String
        Get
            Return m_sBusinessUnitID
        End Get
        Set(ByVal Value As String)
            m_sBusinessUnitID = Value
        End Set
    End Property

    Public Property BusinessUnitOM() As String
        Get
            Return m_sBusinessUnitOM
        End Get
        Set(ByVal Value As String)
            m_sBusinessUnitOM = Value
        End Set
    End Property

    Public Property CustomerID() As String
        Get
            Return m_sCustomerID
        End Get
        Set(ByVal Value As String)
            m_sCustomerID = Value
        End Set
    End Property

    Public Property EmployeeID() As String
        Get
            Return m_sEmployeeID
        End Get
        Set(ByVal Value As String)
            m_sEmployeeID = Value
        End Set
    End Property

    Public Property Addressee() As String
        Get
            Return m_sAddressee
        End Get
        Set(ByVal Value As String)
            m_sAddressee = Value
        End Set
    End Property

    Public ReadOnly Property BackupRecipientIDs() As ArrayList
        Get
            If (m_arrEmpIDs Is Nothing) Then
                m_arrEmpIDs = New ArrayList
            End If
            Return m_arrEmpIDs
        End Get
    End Property

    Public Property WorkOrderNumber() As String
        Get
            Return m_workOrderNo
        End Get
        Set(value As String)
            m_workOrderNo = value
        End Set
    End Property

    Public Property OrderOrigin() As String
        Get
            Return m_orderOrigin
        End Get
        Set(value As String)
            m_orderOrigin = value
        End Set
    End Property

    Public Property BuyerId() As String
        Get
            Return m_Buyer_Id
        End Get
        Set(value As String)
            m_Buyer_Id = value
        End Set
    End Property

    Public Property BuyerEmail() As String
        Get
            Return m_Buyer_Email
        End Get
        Set(value As String)
            m_Buyer_Email = value
        End Set
    End Property

    Public Property PriceBlockFlag() As String
        Get
            Return m_priceblock
        End Get
        Set(ByVal value As String)
            m_priceblock = value
        End Set
    End Property

    Public Property ApprovalLimit() As Decimal
        Get
            Return m_ApprovalLimit
        End Get
        Set(value As Decimal)
            m_ApprovalLimit = value
        End Set
    End Property

    '
    ' checks whether the primary recipient already exist or not
    '
    Public Function IsPrimaryRecipientExist() As Boolean
        Dim bRet As Boolean = True

        ' check for ID value
        bRet = m_sEmployeeID.Trim.Length > 0

        ' check for the Name value
        If bRet Then
            bRet = m_sAddressee.Trim.Length > 0
        End If

        ' check for the email address value
        If bRet Then
            bRet = m_sTO.Trim.Length > 0
        End If

        Return bRet
    End Function

End Class
