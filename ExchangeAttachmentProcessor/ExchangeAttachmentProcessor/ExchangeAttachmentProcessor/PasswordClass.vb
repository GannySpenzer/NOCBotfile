Imports System.IO

Public Class PasswordClass
    Private myTripleDES As TripleDES

    Dim m_bIsEncrypted As Boolean
    Dim m_sPassword As String

    Private Sub EncryptPassword()
        Dim baReturn As Byte()
        Dim encoding As System.Text.Encoding = System.Text.Encoding.UTF8

        baReturn = myTripleDES.Encrypt(Password)
        Password = ConvertToHex(baReturn)

        IsEncrypted = True
    End Sub
    Public Function EncryptPassword(ByVal Password As String) As String
        Dim encoding As System.Text.Encoding = System.Text.Encoding.UTF8
        Dim baDecrypt As Byte()



        baDecrypt = myTripleDES.Encrypt(Password)
        Password = ConvertToHex(baDecrypt)
        Return Password

        IsEncrypted = True
    End Function

    Public Function DecryptPassword(ByVal HexPassword As String) As String
        Dim encoding As System.Text.Encoding = System.Text.Encoding.UTF8
        Dim baDecrypt As Byte()
        Dim sPassword As String

        baDecrypt = ConvertToByteArray(HexPassword)
        sPassword = myTripleDES.Decrypt(baDecrypt)

        Return sPassword

        IsEncrypted = False
    End Function



    Public Property IsEncrypted() As Boolean
        Get
            IsEncrypted = m_bIsEncrypted
        End Get
        Set(ByVal value As Boolean)
            m_bIsEncrypted = value
        End Set
    End Property
    Public Property Password() As String
        Get
            Password = m_sPassword
        End Get
        Set(ByVal value As String)
            m_sPassword = value
        End Set
    End Property

    Private Function ConvertToHex(ByVal baConvertFrom As Byte()) As String
        Dim sReturn As String = ""

        For Each b As Byte In baConvertFrom
            If b < &H10 Then
                Debug.Print("0" & Hex(b).ToString())
                sReturn &= "0" & Hex(b).ToString()
            Else
                Debug.Print(Hex(b).ToString())
                sReturn &= Hex(b).ToString()
            End If


        Next
        Return sReturn

    End Function

    Private Function ConvertToByteArray(ByVal sInput As String) As Byte()

        Dim iCount As Integer
        Dim bTemp As Byte
        Dim sTemp As String
        Dim iArrayIndex As Integer = 0
        Dim iArraySize As Integer

        iArraySize = sInput.Length / 2
        Dim baReturn(iArraySize - 1) As Byte



        For iCount = 0 To sInput.Length - 1 Step 2
            sTemp = sInput.Substring(iCount, 2)
            bTemp = Convert.ToByte(sTemp, 16)
            'If IsNothing(baReturn) Then


            '    baReturn.SetValue(bTemp, 0)
            'Else
            baReturn.SetValue(bTemp, iArrayIndex)
            iArrayIndex += 1
            'End If

        Next


        Return baReturn

    End Function

    Public Sub New()

        myTripleDES = New TripleDES

    End Sub
End Class
