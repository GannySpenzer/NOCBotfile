Public Class Utility

    ' just check if we have the actual email "address" AND the "domain"
    ' part of the email split by the "@" symbol and no part is an empty string
    Public Shared Function IsValidEmailAdd(ByVal cEmailAdd As String) As Boolean
        Try
            Dim bValid As Boolean = False

            If cEmailAdd.Trim.Length > 0 Then
                Dim cParts As String() = cEmailAdd.Split(CType("@", Char))

                If cParts.Length = 2 Then
                    bValid = (cParts(0).Length > 0 And cParts(1).Length > 0)
                Else
                    bValid = False
                End If
            Else
                bValid = False
            End If

            Return bValid

        Catch ex As Exception
            Throw New Exception(ex.ToString)
        End Try
    End Function

    Public Shared Function FormatAddessee(ByVal cAddressee As String) As String
        Try
            Dim cReturn As String = ""

            If cAddressee.Trim.Length > 0 Then
                Dim cParts As String() = cAddressee.Split(CType(",", Char))

                If cParts.Length > 1 Then
                    For nCtr As Integer = 0 To (cParts.Length - 1)
                        If cParts(nCtr).Trim.Length > 0 Then
                            ' append the string (name)
                            cReturn &= cParts(nCtr)

                            ' add comma and a space for proper formatting
                            If Not (nCtr = (cParts.Length - 1)) Then
                                cReturn &= ", "
                            End If
                        End If
                    Next
                Else
                    cReturn = cAddressee
                End If
            End If

            Return cReturn

        Catch ex As Exception
            Throw New Exception(ex.ToString)
        End Try
    End Function

End Class
