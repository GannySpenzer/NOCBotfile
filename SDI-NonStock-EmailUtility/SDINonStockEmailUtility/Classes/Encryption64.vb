Imports System.IO
Imports System.Text
Imports System.Security.Cryptography

Public Class Encryption64

    Private key() As Byte = {}
    Private IV() As Byte = {&H12, &H34, &H56, &H78, &H90, &HAB, &HCD, &HEF}
    Private sDefaultKey As String = "#sdi.default"

    Public Function Decrypt(ByVal stringToDecrypt As String, ByVal sEncryptionKey As String) As String
        Try
            Dim inputByteArray(stringToDecrypt.Length) As Byte
            Dim sKey As String = sEncryptionKey.Trim

            If sKey.Length > 0 Then
                If sKey.Length < 8 Then
                    sKey = sKey.PadRight(8, CType("%", Char))
                End If
            Else
                sKey = sDefaultKey
            End If

            key = System.Text.Encoding.UTF8.GetBytes(sKey.Substring(0, 8))

            Dim des As New DESCryptoServiceProvider

            inputByteArray = Convert.FromBase64String(stringToDecrypt)

            Dim ms As New MemoryStream
            Dim cs As New CryptoStream(ms, des.CreateDecryptor(key, IV), CryptoStreamMode.Write)

            cs.Write(inputByteArray, 0, inputByteArray.Length)
            cs.FlushFinalBlock()

            Return System.Text.Encoding.UTF8.GetString(ms.ToArray())

        Catch e As Exception
            Throw e
        End Try
    End Function

    Public Function Encrypt(ByVal stringToEncrypt As String, ByVal SEncryptionKey As String) As String
        Try
            Dim sKey As String = SEncryptionKey.Trim

            If sKey.Length > 0 Then
                If sKey.Length < 8 Then
                    sKey = sKey.PadRight(8, CType("%", Char))
                End If
            Else
                sKey = sDefaultKey
            End If

            key = System.Text.Encoding.UTF8.GetBytes(sKey.Substring(0, 8))

            Dim des As New DESCryptoServiceProvider
            Dim inputByteArray() As Byte = System.Text.Encoding.UTF8.GetBytes(stringToEncrypt)
            Dim ms As New MemoryStream
            Dim cs As New CryptoStream(ms, des.CreateEncryptor(key, IV), CryptoStreamMode.Write)

            cs.Write(inputByteArray, 0, inputByteArray.Length)
            cs.FlushFinalBlock()

            Return Convert.ToBase64String(ms.ToArray())

        Catch e As Exception
            Throw e
        End Try
    End Function

End Class
