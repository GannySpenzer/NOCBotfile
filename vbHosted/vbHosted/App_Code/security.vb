Imports Microsoft.VisualBasic

Public Class security
    Const SECRET_KEY As String = "f1fda39cc52f40c9b1bac28a025127354f6d536e2009403ba1ff68d5aac70598890f959f312f45f1af7f45a8e7a89b99aa8e3cf6519041358e301170994db96349daa1836a404fb89bde7a5f3f57a495bfe6594cebbb4a0fb79f76f1afafc3775e6e7e656f0a46c8a7d203923adafaf57914188823344029b4d3f42198c55848"

    Public Shared Function sign(ByVal paramsArray As Hashtable) As String
        Return sign(buildDataToSign(paramsArray), SECRET_KEY)
    End Function

    Private Shared Function sign(ByVal data As String, ByVal secretKey As String) As String

        Dim encoder As New System.Text.UTF8Encoding
        Dim key() As Byte = encoder.GetBytes(secretKey)
        Dim dataAsBytes() As Byte = encoder.GetBytes(data)

        Dim myHMACSHA256 As New System.Security.Cryptography.HMACSHA256(key)
        Dim HashCode As Byte() = myHMACSHA256.ComputeHash(dataAsBytes)

        Return Convert.ToBase64String(HashCode)

    End Function

    Private Shared Function buildDataToSign(ByVal paramsArray As Hashtable) As String
        Dim signedFieldNames As String() = paramsArray("signed_field_names").Split(",")
        Dim dataToSign As New List(Of String)
        Dim signedFieldName As String

        For Each signedFieldName In signedFieldNames
            dataToSign.Add(signedFieldName + "=" + paramsArray(signedFieldName))
        Next

        Return commaSeparate(dataToSign)
    End Function

    Private Shared Function commaSeparate(ByVal dataToSign As List(Of String)) As String
        Return String.Join(",", dataToSign.ToArray)
    End Function

End Class
