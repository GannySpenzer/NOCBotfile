Imports System.Data.OleDb
Public Class clsQuote
    Private strORDER_NO As String
    Public ReadOnly Property ORDER_NO() As String
        Get
            Return strORDER_NO
        End Get
    End Property
    Private strISA_IDENTIFIER As String
    Public ReadOnly Property ISA_IDENTIFIER() As String
        Get
            Return strISA_IDENTIFIER
        End Get
    End Property
    Private strLIne_NBR As String
    Public ReadOnly Property LIne_NBR() As String
        Get
            Return strLIne_NBR
        End Get
    End Property
    Private strISA_PARENT_IDENT As String
    Public ReadOnly Property ISA_PARENT_IDENT() As String
        Get
            Return strISA_PARENT_IDENT
        End Get
    End Property
    Private intNET_UNIT_PRICE As Double
    Public ReadOnly Property NET_UNIT_PRICE() As Double
        Get
            Return intNET_UNIT_PRICE
        End Get
    End Property
    Private strVendor_ID As String
    Public ReadOnly Property Vendor_ID() As String
        Get
            Return strVendor_ID
        End Get
    End Property
    Private strQTY_REQ As String
    Public ReadOnly Property QTY_REQ() As String
        Get
            Return strQTY_REQ
        End Get
    End Property
    Private strISA_WORK_ORDER_NO As String
    Public ReadOnly Property ISA_WORK_ORDER_NO() As String
        Get
            Return strISA_WORK_ORDER_NO
        End Get
    End Property
    Private strISA_ORDER_STATUS As String
    Public ReadOnly Property ISA_ORDER_STATUS() As String
        Get
            Return strISA_ORDER_STATUS
        End Get
    End Property
    Private strISA_FLG_PUNCHOUT As String
    Public ReadOnly Property ISA_FLG_PUNCHOUT() As String
        Get
            Return strISA_FLG_PUNCHOUT
        End Get
    End Property
    Private intNet_PRICE_PO As Double
    Public ReadOnly Property Net_PRICE_PO() As Double
        Get
            Return intNet_PRICE_PO
        End Get
    End Property
    Private strITM_ID_VNDR As String
    Public ReadOnly Property ITM_ID_VNDR() As String
        Get
            Return strITM_ID_VNDR
        End Get
    End Property
    Private strITM_ID_VNDR_AUX As String
    Public ReadOnly Property ITM_ID_VNDR_AUX() As String
        Get
            Return strITM_ID_VNDR_AUX
        End Get
    End Property
    Private strVNDR_CATALOG_ID As String
    Public ReadOnly Property VNDR_CATALOG_ID() As String
        Get
            Return strVNDR_CATALOG_ID
        End Get
    End Property
    Private strMFG_ID As String
    Public ReadOnly Property MFG_ID() As String
        Get
            Return strMFG_ID
        End Get
    End Property
    Private strMFG_ITM_ID As String
    Public ReadOnly Property MFG_ITM_ID() As String
        Get
            Return strMFG_ITM_ID
        End Get
    End Property
    Private strSHIPTO_ID As String
    Public ReadOnly Property SHIPTO_ID() As String
        Get
            Return strSHIPTO_ID
        End Get
    End Property
    Private strVNDR_LOC As String
    Public ReadOnly Property VNDR_LOC() As String
        Get
            Return strVNDR_LOC
        End Get
    End Property
    Private strISA_REQUIRED_BY_DT As Date
    Public ReadOnly Property ISA_REQUIRED_BY_DT() As Date
        Get
            Return strISA_REQUIRED_BY_DT
        End Get
    End Property
    Private strISA_Invalid_Ref_NO As String
    Public ReadOnly Property ISA_Invalid_Ref_NO() As String
        Get
            Return strISA_Invalid_Ref_NO
        End Get
    End Property





    Public Sub New(ByVal strorderno As String, ByVal strlineno As String, ByVal connectOR As OleDbConnection)
        Dim strSQLstring As String
        strSQLstring = "Select " & vbCrLf & _
                                                       " H.Order_no, " & vbCrLf & _
                                                       " L.ISA_INTFC_LN AS LIne_NBR," & vbCrLf & _
                                                       " H.Order_no AS ISA_IDENTIFIER, " & vbCrLf & _
                                                       " L.Order_no AS ISA_PARENT_IDENT," & vbCrLf & _
                                                       " L.ISA_SELL_PRICE AS NET_UNIT_PRICE," & vbCrLf & _
                                                       " L.ISA_REQUIRED_BY_DT," & vbCrLf & _
                                                       " L.PRICE_VNDR AS PRICE_PO," & vbCrLf & _
                                                       " L.Vendor_ID, " & vbCrLf & _
                                                       " L.ITM_ID_VNDR, L.ISA_USER5, " & vbCrLf & _
                                                       " '1' AS VNDR_LOC," & vbCrLf & _
                                                       " ' ' AS VNDR_CATALOG_ID," & vbCrLf & _
                                                       " L.SHIPTO_ID," & vbCrLf & _
                                                       " L.MFG_ID," & vbCrLf & _
                                                       " L.MFG_ITM_ID," & vbCrLf & _
                                                       " L.QTY_REQUESTED AS QTY_REQ," & vbCrLf & _
                                                       " L.ISA_WORK_ORDER_NO," & vbCrLf & _
                                                       " L.ISA_LINE_STATUS AS ISA_ORDER_STATUS," & vbCrLf & _
                                                       " ' ' AS ISA_FLG_PUNCHOUT" & vbCrLf & _
                                                      "  from SYSADM8.PS_ISA_ORD_INTF_LN L ," & vbCrLf & _
                                                      "  SYSADM8.PS_ISA_ORD_INTF_HD H " & vbCrLf & _
                                                      "  where H.order_no ='" & strorderno & "'" & vbCrLf & _
                                                      "  and l.ISA_INTFC_LN='" & strlineno & "'" & vbCrLf & _
                                                      "  and h.BUSINESS_UNIT_OM = l.BUSINESS_UNIT_OM" & vbCrLf & _
                                                      "  and h.order_no = l.order_no"

        Dim command1 As OleDbCommand
        command1 = New OleDbCommand(strSQLstring, connectOR)
        Dim objReader As OleDbDataReader = Nothing
        strISA_Invalid_Ref_NO = ""

        Try
            objReader = command1.ExecuteReader()

            If objReader.Read() Then
                strORDER_NO = objReader.Item("Order_no")
                strLIne_NBR = objReader.Item("LIne_NBR")
                strISA_IDENTIFIER = objReader.Item("ISA_IDENTIFIER")
                strISA_PARENT_IDENT = objReader.Item("ISA_PARENT_IDENT")
                strQTY_REQ = objReader.Item("QTY_REQ")
                strISA_WORK_ORDER_NO = objReader.Item("ISA_WORK_ORDER_NO")
                strISA_ORDER_STATUS = objReader.Item("ISA_ORDER_STATUS")
                strISA_FLG_PUNCHOUT = objReader.Item("ISA_FLG_PUNCHOUT")
                intNET_UNIT_PRICE = objReader.Item("NET_UNIT_PRICE")
                strVendor_ID = objReader.Item("Vendor_ID")
                strVNDR_CATALOG_ID = objReader.Item("VNDR_CATALOG_ID")
                intNet_PRICE_PO = objReader.Item("PRICE_PO")
                strMFG_ITM_ID = objReader.Item("MFG_ITM_ID")
                strMFG_ID = objReader.Item("MFG_ID")
                strITM_ID_VNDR = objReader.Item("ITM_ID_VNDR")
                strITM_ID_VNDR_AUX = objReader.Item("ISA_USER5")
                strSHIPTO_ID = objReader.Item("SHIPTO_ID")
                strVNDR_LOC = objReader.Item("VNDR_LOC")
                strISA_REQUIRED_BY_DT = objReader.Item("ISA_REQUIRED_BY_DT")
                strISA_Invalid_Ref_NO = ""
            Else
                strISA_Invalid_Ref_NO = "Invalid Ref Order NO: " & strorderno
            End If
            objReader.Close()
        Catch ex As Exception
            strISA_Invalid_Ref_NO = "Invalid Ref Order NO: " & strorderno
            If Not objReader Is Nothing Then
                If objReader.HasRows Then
                    objReader.Close()
                End If
                objReader = Nothing
            End If
        End Try

    End Sub


End Class
