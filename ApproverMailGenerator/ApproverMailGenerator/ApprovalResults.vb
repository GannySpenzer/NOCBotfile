Public Class ApprovalResults
    Public Enum ApprovalError
        NoError = 0
        InvalidApprovalChain
    End Enum

    Private m_oEmployee As EmployeeLevel
    Private m_oChargeCode As ChargeCodeLevel
    Private m_bNeedsQuote As Boolean
    Private m_eError As ApprovalError

    Public Sub New()
        m_oEmployee = New EmployeeLevel
        m_oChargeCode = New ChargeCodeLevel
        m_bNeedsQuote = False
        m_eError = ApprovalError.NoError
    End Sub

    Public Sub UpdateEmployeeResults(bExceededLimit, sNewHeaderStatus, sNextApproverID)
        m_oEmployee.ExceededLimit = bExceededLimit
        m_oEmployee.NewHeaderStatus = sNewHeaderStatus
        m_oEmployee.NextApproverID = sNextApproverID
    End Sub

    Public Property OrderExceededLimit() As Boolean
        Get
            Return m_oEmployee.ExceededLimit
        End Get
        Set(value As Boolean)
            m_oEmployee.ExceededLimit = value
        End Set
    End Property

    Public Property ErrorInApproval As ApprovalError
        Get
            Return m_eError
        End Get
        Set(value As ApprovalError)
            m_eError = value
        End Set
    End Property

    Public Function IsAnyChargeCodeExceededLimit() As Boolean
        Return m_oChargeCode.IsAnyExceededLimit
    End Function

    Public Property NextOrderApprover() As String
        Get
            Return m_oEmployee.NextApproverID
        End Get
        Set(value As String)
            m_oEmployee.NextApproverID = value
        End Set
    End Property

    Public Property NewOrderHeaderStatus() As String
        Get
            Return m_oEmployee.NewHeaderStatus
        End Get
        Set(value As String)
            m_oEmployee.NewHeaderStatus = value
        End Set
    End Property

    Public Function OrderChargeCode() As String
        Return m_oEmployee.ChargeCode
    End Function

    Public Function IsMoreApproversNeeded() As Boolean
        Dim bMoreApproversNeeded As Boolean = False

        If m_oEmployee.ExceededLimit Then
            bMoreApproversNeeded = True
        ElseIf m_oChargeCode.IsAnyExceededLimit Then
            bMoreApproversNeeded = True
        End If

        Return bMoreApproversNeeded
    End Function

    Public Function BudgetChargeCodesCount() As Integer
        Return m_oChargeCode.ChargeCodesCount
    End Function

    Public Property BudgetExceededLimit(iIndex As Integer) As Boolean
        Get
            Dim bExceeded As Boolean = False

            If IsValidBudgetIndex(iIndex) Then
                bExceeded = m_oChargeCode.ExceededLimit(iIndex)
            End If

            Return bExceeded
        End Get
        Set(value As Boolean)
            If IsValidBudgetIndex(iIndex) Then
                m_oChargeCode.ExceededLimit(iIndex) = value
            End If
        End Set
    End Property

    Public Property NextBudgetApprover(iIndex As Integer) As String
        Get
            Dim sApproverID As String = ""

            If IsValidBudgetIndex(iIndex) Then
                sApproverID = m_oChargeCode.NextApproverID(iIndex)
            End If

            Return sApproverID
        End Get
        Set(value As String)
            If IsValidBudgetIndex(iIndex) Then
                m_oChargeCode.NextApproverID(iIndex) = value
            End If
        End Set
    End Property

    Public Property NewBudgetHeaderStatus(iIndex As Integer) As String
        Get
            Dim sHeaderStatus As String = ""

            If IsValidBudgetIndex(iIndex) Then
                sHeaderStatus = m_oChargeCode.NewHeaderStatus(iIndex)
            End If

            Return sHeaderStatus
        End Get
        Set(value As String)
            If IsValidBudgetIndex(iIndex) Then
                m_oChargeCode.NewHeaderStatus(iIndex) = value
            End If
        End Set
    End Property

    Public Function BudgetChargeCode(iIndex As Integer) As String
        Dim sChargeCode As String = ""

        If IsValidBudgetIndex(iIndex) Then
            sChargeCode = m_oChargeCode.ChargeCode(iIndex)
        End If

        Return sChargeCode
    End Function

    Private Function IsValidBudgetIndex(iIndex As Integer) As Boolean
        Dim bValidIndex As Boolean = False

        If iIndex >= 0 And iIndex < m_oChargeCode.ChargeCodesCount Then
            bValidIndex = True
        End If

        Return bValidIndex
    End Function

    Public Sub InitBudgetChargeCodes(arrChgCodes As ArrayList)
        m_oChargeCode = New ChargeCodeLevel(arrChgCodes)
    End Sub

    Public Property NeedsQuote As Boolean
        Get
            Return m_bNeedsQuote
        End Get
        Set(value As Boolean)
            m_bNeedsQuote = value
        End Set
    End Property

    Private Class EmployeeLevel
        Private m_bExceededLimit As Boolean
        Private m_sNextApproverID As String
        Private m_sNewHeaderStatus As String
        Private m_sChgCode As String

        Public Sub New()
            m_bExceededLimit = False
            m_sNextApproverID = ""
            m_sNewHeaderStatus = ""
            m_sChgCode = "NotChgCode"
        End Sub

        Public Property ExceededLimit As Boolean
            Get
                Return m_bExceededLimit
            End Get
            Set(value As Boolean)
                m_bExceededLimit = value
            End Set
        End Property

        Public Property NewHeaderStatus As String
            Get
                Return m_sNewHeaderStatus
            End Get
            Set(value As String)
                m_sNewHeaderStatus = value
            End Set
        End Property

        Public Property NextApproverID As String
            Get
                Return m_sNextApproverID
            End Get
            Set(value As String)
                m_sNextApproverID = value
            End Set
        End Property

        Public Property ChargeCode As String
            Get
                Return m_sChgCode
            End Get
            Set(value As String)
                m_sChgCode = value
            End Set
        End Property
    End Class

    Private Class ChargeCodeLevel
        Private m_bExceededLimit() As Boolean
        Private m_sNextApproverID() As String
        Private m_sNewHeaderStatus() As String
        Private m_arrChgCode As ArrayList

        Public Sub New()
            m_bExceededLimit = New Boolean(0) {}
            m_bExceededLimit(0) = False

            m_sNextApproverID = New String(0) {}
            m_sNextApproverID(0) = ""

            m_sNewHeaderStatus = New String(0) {}
            m_sNewHeaderStatus(0) = ""

            m_arrChgCode = New ArrayList
            m_arrChgCode.Add("")
        End Sub

        Public Sub New(arrChargeCodes As ArrayList)
            m_arrChgCode = New ArrayList
            m_arrChgCode.Clear()
            m_arrChgCode.AddRange(arrChargeCodes)

            m_bExceededLimit = New Boolean(m_arrChgCode.Count - 1) {}
            m_sNextApproverID = New String(m_arrChgCode.Count - 1) {}
            m_sNewHeaderStatus = New String(m_arrChgCode.Count - 1) {}

        End Sub

        Public Function ChargeCodesCount() As Integer
            Return m_arrChgCode.Count
        End Function

        Public Function IsAnyExceededLimit() As Boolean
            Dim bExceeded As Boolean = False
            Dim I As Integer = 0

            While I < m_bExceededLimit.Length And Not bExceeded
                If m_bExceededLimit(I) Then
                    bExceeded = True
                End If
                I = I + 1
            End While

            Return bExceeded
        End Function

        Public Property ExceededLimit(iIndex As Integer) As Boolean
            Get
                Return m_bExceededLimit(iIndex)
            End Get
            Set(value As Boolean)
                m_bExceededLimit(iIndex) = value
            End Set
        End Property

        Public Property NextApproverID(iIndex As Integer) As String
            Get
                Return m_sNextApproverID(iIndex)
            End Get
            Set(value As String)
                m_sNextApproverID(iIndex) = value
            End Set
        End Property

        Public Property NewHeaderStatus(iIndex As Integer) As String
            Get
                Return m_sNewHeaderStatus(iIndex)
            End Get
            Set(value As String)
                m_sNewHeaderStatus(iIndex) = value
            End Set
        End Property

        Public ReadOnly Property ChargeCode(iIndex As Integer) As String
            Get
                Return m_arrChgCode(iIndex)
            End Get
        End Property
    End Class
End Class
