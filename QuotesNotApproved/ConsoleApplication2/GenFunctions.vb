Imports System.Security.Cryptography
Imports System.Text

Namespace GeneralFunctions

    Public Class GenFunctions

        Public Shared Function decryptQueryString(ByVal strQueryString)

            Dim oES As Encryption64 = New Encryption64
            'Return oES.Decrypt(strQueryString, "!#$a54?3")
            Return oES.Decrypt(strQueryString, "b?50$#@!")

        End Function

        Public Shared Function encryptQueryString(ByVal strQueryString) As String

            Dim oES As Encryption64 = New Encryption64
            'Return oES.Encrypt(strQueryString, "!#$a54?3")
            Return oES.Encrypt(strQueryString, "b?50$#@!")

        End Function

        Public Shared Function Stripquote(ByVal sValue)

            Dim icount
            Dim sTemp
            Dim sTemp2

            sTemp2 = sValue

            icount = 1

            Do While icount <= Len(sValue)

                If Not (Asc(Mid(sValue, icount, 1)) = 39) Then
                    sTemp = sTemp & Mid(sValue, icount, 1)
                End If

                icount = icount + 1
            Loop

            If Len(sTemp) = 0 Then
                sTemp = sTemp2
            End If

            Return sTemp

        End Function

    End Class
End Namespace