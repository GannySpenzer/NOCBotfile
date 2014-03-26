Public Class Utility

    ' just check if we have the actual email "address" AND the "domain"
    ' part of the email split by the "@" symbol and no part is an empty string
    Public Shared Function IsValidEmailAdd(ByVal cEmailAdd As String) As Boolean
        'Try
        '    Dim bValid As Boolean = False

        '    If cEmailAdd.Trim.Length > 0 Then
        '        Dim cParts As String() = cEmailAdd.Split(CType("@", Char))

        '        If cParts.Length = 2 Then
        '            bValid = (cParts(0).Length > 0 And cParts(1).Length > 0)
        '        Else
        '            bValid = False
        '        End If
        '    Else
        '        bValid = False
        '    End If

        '    Return bValid

        'Catch ex As Exception
        '    Throw New Exception(ex.ToString)
        'End Try
        Return (ExtractValidEmails(cEmailAdd).Count > 0)
    End Function

    Private Shared Function IsValidSingleAdd(ByVal SingleEmailAdd As String) As Boolean
        Try
            Dim bValid As Boolean = False

            If SingleEmailAdd.Trim.Length > 0 Then
                Dim cParts As String() = SingleEmailAdd.Split(CType("@", Char))

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
            Throw New Exception("IsValidSingleAdd::" & ex.ToString)
        End Try
    End Function

    '
    ' searches for any valid email address within the passed string
    ' returns TRUE (if one was found) or FALSE (if none was found)
    '
    Private Shared Function IsAnyValidEmail(ByVal Addresses As String) As Boolean
        Try
            Dim bValidEmailExist As Boolean = False

            If Addresses.Length > 0 Then
                Dim arrAddress As String() = Addresses.Split(CType(";", Char))

                If arrAddress.Length > 0 Then
                    For Each sAdd As String In arrAddress
                        If sAdd.Trim.Length > 0 Then
                            ' contains something, let's check if a valid email address
                            bValidEmailExist = IsValidSingleAdd(sAdd.Trim)
                        Else
                            ' empty string, do nothing!!!
                        End If

                        If bValidEmailExist Then
                            Exit For
                        End If
                    Next
                End If
            End If

            Return bValidEmailExist

        Catch ex As Exception
            Throw New Exception("IsAnyValidEmail::" & ex.ToString)
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

    Public Shared Function ExtractValidEmails(ByVal EmailAddresses As String) As ArrayList
        Dim arrEmails As New ArrayList

        If EmailAddresses.Trim.Length > 0 Then

            ' parse into individual address
            Dim arrEach As String() = EmailAddresses.Split(CType(";", Char))

            ' process each
            If arrEach.Length > 0 Then
                For Each emailAdd As String In arrEach
                    If emailAdd.Trim.Length > 0 Then
                        ' check and add into array if valid
                        If IsValidSingleAdd(emailAdd.Trim) Then
                            arrEmails.Add(emailAdd.Trim)
                        End If
                    End If
                Next
            End If

        End If

        Return arrEmails
    End Function

End Class
