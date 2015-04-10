Imports System.Net
Imports System.Security.Cryptography.X509Certificates

Public Class AlwaysIgnoreCertPolicy
    Implements ICertificatePolicy
    Public Function CheckValidationResult(ByVal srvPoint As ServicePoint, _
                                          ByVal cert As X509Certificate, ByVal request As WebRequest, _
                                          ByVal certificateProblem As Integer) As Boolean Implements ICertificatePolicy.CheckValidationResult
        ' return TRUE to force the certificate to be accepted.
        Return True
    End Function
End Class