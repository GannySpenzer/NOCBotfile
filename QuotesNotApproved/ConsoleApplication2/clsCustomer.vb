Imports System.Data.OleDb
Public Class clsCustomer

    Private strName1 As String
    Public ReadOnly Property Name1() As String
        Get
            Return strName1
        End Get
    End Property

    Private strPhone As String
    Public ReadOnly Property Phone() As String
        Get
            Return strPhone
        End Get
    End Property

    Private strFax As String
    Public ReadOnly Property Fax() As String
        Get
            Return strFax
        End Get
    End Property

    Public Sub New(ByVal CustId As String)
        Dim strSQLstring As String

        strSQLstring = "SELECT A.NAME1, A.NAME2, A.NAME3," & vbCrLf & _
                    " B.ADDRESS1, B.ADDRESS2, B.ADDRESS3," & vbCrLf & _
                    " B.ADDRESS4, B.CITY, B.STATE, B.POSTAL," & vbCrLf & _
                    " B.PHONE, B.EXTENSION, B.FAX" & vbCrLf & _
                    " FROM PS_CUSTOMER A, PS_CUST_ADDRESS B" & vbCrLf & _
                    " WHERE A.SETID = 'MAIN1'" & vbCrLf & _
                    " AND A.CUST_ID = '" & CustId & "'" & vbCrLf & _
                    " AND A.SETID = B.SETID" & vbCrLf & _
                    " AND A.CUST_ID = B.CUST_ID" & vbCrLf & _
                    " AND A.ADDRESS_SEQ_SHIP = B.ADDRESS_SEQ_NUM" & vbCrLf & _
                    " AND B.EFFDT =" & vbCrLf & _
                    " (SELECT MAX(B_ED.EFFDT) FROM PS_CUST_ADDRESS B_ED" & vbCrLf & _
                    " WHERE B.SETID = B_ED.SETID" & vbCrLf & _
                    " AND B.CUST_ID = B_ED.CUST_ID" & vbCrLf & _
                    " AND B.ADDRESS_SEQ_NUM = B_ED.ADDRESS_SEQ_NUM" & vbCrLf & _
                    " AND B_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                    " AND B.EFF_STATUS = 'A'"


        Dim objReaderReq As OleDbDataReader = ORDBData.GetReader(strSQLstring)
        If objReaderReq.Read() Then
            strName1 = objReaderReq.Item("NAME1")
            strPhone = objReaderReq.Item("PHONE")
            strFax = objReaderReq.Item("FAX")
        End If
        objReaderReq.Close()
    End Sub

End Class
