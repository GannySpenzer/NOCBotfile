Imports System.Data.OleDb
Public Class clsEnterprise
    Private strCustID As String
    Public ReadOnly Property CustID() As String
        Get
            Return strCustID
        End Get
    End Property
    Private strCompanyID As String
    Public ReadOnly Property CompanyID() As String
        Get
            Return strCompanyID
        End Get
    End Property

    Private intLastItemID As Int32
    Public ReadOnly Property LastItemID() As Int32
        Get
            Return intLastItemID
        End Get
    End Property

    Private intItemIDLen As Integer
    Public ReadOnly Property ItemIDLen() As Int32
        Get
            Return intItemIDLen
        End Get
    End Property

    Private strItemMode As String
    Public ReadOnly Property ItemMode() As String
        Get
            Return strItemMode
        End Get
    End Property

    Private strNONSKREQEmail As String
    Public ReadOnly Property NONSKREQEmail() As String
        Get
            Return strNONSKREQEmail
        End Get
    End Property

    Private strItemAddEmail As String
    Public ReadOnly Property ItemAddEmail() As String
        Get
            Return strItemAddEmail
        End Get
    End Property

    Private strItemAddPrinter As String
    Public ReadOnly Property ItemAddPrinter() As String
        Get
            Return strItemAddPrinter
        End Get
    End Property

    Private strLastUPDOprid As String
    Public ReadOnly Property LastUPDOprid() As String
        Get
            Return strLastUPDOprid
        End Get
    End Property

    Private strOrdApprType As String
    Public ReadOnly Property OrdApprType() As String
        Get
            Return strOrdApprType
        End Get
    End Property

    Private strOrdBudgetFlg As String
    Public ReadOnly Property OrdBudgetFlg() As String
        Get
            Return strOrdBudgetFlg
        End Get
    End Property

    Private strReceivingDate As String
    Public ReadOnly Property ReceivingDate() As String
        Get
            Return strReceivingDate
        End Get
    End Property

    Private strCustPrfxFlag As String
    Public ReadOnly Property CustPrfxFlag() As String
        Get
            Return strCustPrfxFlag
        End Get
    End Property

    Private strCustPrefix As String
    Public ReadOnly Property CustPrefix() As String
        Get
            Return strCustPrefix
        End Get
    End Property

    Private strShopCartPage As String
    Public ReadOnly Property ShopCartPage() As String
        Get
            Return strShopCartPage
        End Get
    End Property

    Private strLPPFlag As String
    Public ReadOnly Property LPPFlag() As String
        Get
            Return strLPPFlag
        End Get
    End Property

    Private strTaxFlag As String
    Public ReadOnly Property TaxFlag() As String
        Get
            Return strTaxFlag
        End Get
    End Property


    Public Sub New(ByVal BusinessUnit As String)
        Dim strSQLstring As String
        strSQLstring = "SELECT A.ISA_COMPANY_ID," & vbCrLf & _
                        " A.ISA_LASTITEMID," & vbCrLf & _
                        " A.ISA_ITEMID_LEN," & vbCrLf & _
                        " A.ISA_ITEMID_MODE," & vbCrLf & _
                        " A.ISA_ITEMADD_EMAIL," & vbCrLf & _
                        " A.ISA_NONSKREQ_EMAIL," & vbCrLf & _
                        " A.ISA_ITMADD_PRINTER," & vbCrLf & _
                        " A.ISA_ORD_APPR_TYPE," & vbCrLf & _
                        " A.ISA_ORD_BUDGET_FLG," & vbCrLf & _
                        " A.ISA_RECEIVING_DATE," & vbCrLf & _
                        " A.ISA_CUST_PRFX_FLAG," & vbCrLf & _
                        " A.ISA_CUST_PREFIX," & vbCrLf & _
                        " A.ISA_SHOPCART_PAGE," & vbCrLf & _
                        " A.ISA_LPP_FLAG," & vbCrLf & _
                        " A.ISA_ISOL_TAX_FLAG," & vbCrLf & _
                        " A.LASTUPDOPRID" & vbCrLf & _
                        " FROM PS_ISA_ENTERPRISE A" & vbCrLf & _
                        " WHERE A.ISA_BUSINESS_UNIT = '" & BusinessUnit & "'"

        Dim objReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
        If objReader.Read() Then
            strCompanyID = objReader.Item("ISA_COMPANY_ID")
            intLastItemID = objReader.Item("ISA_LASTITEMID")
            intItemIDLen = objReader.Item("ISA_ITEMID_LEN")
            strItemAddEmail = objReader.Item("ISA_ITEMADD_EMAIL")
            strNONSKREQEmail = objReader.Item("ISA_NONSKREQ_EMAIL")
            strItemAddPrinter = objReader.Item("ISA_ITMADD_PRINTER")
            strLastUPDOprid = objReader.Item("LASTUPDOPRID")
            strItemMode = objReader.Item("ISA_ITEMID_MODE")
            strOrdApprType = objReader.Item("ISA_ORD_APPR_TYPE")
            strOrdBudgetFlg = objReader.Item("ISA_ORD_BUDGET_FLG")
            If IsDBNull(objReader.Item("ISA_RECEIVING_DATE")) Then
                strReceivingDate = "NO"
            Else
                strReceivingDate = objReader.Item("ISA_RECEIVING_DATE")
            End If
            If Trim(objReader.Item("ISA_SHOPCART_PAGE")) = "" Then
                strShopCartPage = "ShoppingCart.aspx"
            Else
                strShopCartPage = objReader.Item("ISA_SHOPCART_PAGE")
            End If
            strLPPFlag = objReader.Item("ISA_LPP_FLAG")
            strTaxFlag = objReader.Item("ISA_ISOL_TAX_FLAG")
            strCustPrfxFlag = objReader.Item("ISA_CUST_PRFX_FLAG")
            strCustPrefix = objReader.Item("ISA_CUST_PREFIX")

        End If
    End Sub
End Class
