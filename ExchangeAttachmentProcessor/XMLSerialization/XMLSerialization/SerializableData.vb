Imports System.IO
Imports System.Xml.Serialization

Public Class SerializableData

    Public Shared Function Load(ByVal filename As String, ByVal newType As Type) As Object
        Dim fileInfo As New FileInfo(filename)
        If Not fileInfo.Exists Then
            Return System.Activator.CreateInstance(newType)
        End If
        fileInfo = Nothing
        Dim stream As New FileStream(filename, FileMode.Open)
        Dim newObject As Object = Load(stream, newType)
        stream.Close()
        Return newObject

    End Function

    Public Shared Function Load(ByVal stream As Stream, ByVal newType As Type) As Object
        Dim serializer As New XmlSerializer(newType)
        Dim newObject As Object = serializer.Deserialize(Stream)
        Return newObject

    End Function

    Public Sub Save(ByVal filename As String)

        Dim tempFilename As String
        tempFilename = filename & ".tmp"

        Dim TempFileInfo As New FileInfo(tempFilename)
        If TempFileInfo.Exists Then TempFileInfo.Delete()

        Dim stream As New FileStream(tempFilename, FileMode.Create)

        Save(stream)
        stream.Close()
        stream = Nothing
        TempFileInfo.CopyTo(filename, True)
        TempFileInfo.Delete()
        TempFileInfo = Nothing

    End Sub

    Public Sub Save(ByVal stream As Stream)
        Dim serializer As New XmlSerializer(Me.GetType)

        serializer.Serialize(stream, Me)

    End Sub
End Class
