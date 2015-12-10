Imports System.IO
Imports System.Xml.Serialization

Public Class SerializableData

    Public Shared Function Load(ByVal sFileSpec As String, ByVal newType As Type, ByRef newObject As Object, Optional ByRef sMessage As String = "") As Boolean
        Dim bSuccess As Boolean = False
        Try
            Dim fileInfo As New FileInfo(sFileSpec)
            If Not fileInfo.Exists Then
                sMessage = "ERROR: SerializableData.Load: " & sFileSpec & " does not exist"
                bSuccess = False
            Else
                fileInfo = Nothing
                Dim stream As New FileStream(sFileSpec, FileMode.Open)
                Dim serializer As New XmlSerializer(newType)
                newObject = serializer.Deserialize(stream)
                stream.Close()
                bSuccess = True
            End If

        Catch ex As Exception
            sMessage = "ERROR: SerializableData.Load: Unexpected error - " & ex.Message & " " & ex.StackTrace
            bSuccess = False
        End Try

        Return bSuccess

    End Function

End Class
