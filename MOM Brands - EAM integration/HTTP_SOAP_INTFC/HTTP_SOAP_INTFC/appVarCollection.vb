Public Class appVarCollection

    Public Enum eVarValueField
        [DataType] = 0
        [Value] = 1
    End Enum

    Private m_arr As System.Collections.Hashtable = Nothing

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub AddDefaultVars()
        ' default string settings for HTTP SOAP interface
        Me.AddEntry(key:="{SYSTEM:EMPTY_STRING}", dataType:="STRING", value:=" ")
        Me.AddEntry(key:="{SYSTEM:DATETIME}", dataType:="STRING", value:="SYSDATE")
        Me.AddEntry(key:="{SYSTEM:EMPTY_NUMBER}", dataType:="STRING", value:="0")
    End Sub

    Public Sub AddEntry(ByVal key As String, _
                        ByVal dataType As String, _
                        ByVal value As String)
        If Me.Vars.ContainsKey(key) Then
            ' update
            Me.Vars(key) = New String() {dataType, value}
        Else
            ' create new
            Me.Vars.Add(key, New String() {dataType, value})
        End If
    End Sub

    Public ReadOnly Property [Vars] As System.Collections.Hashtable
        Get
            If (m_arr Is Nothing) Then
                m_arr = New System.Collections.Hashtable
            End If
            Return m_arr
        End Get
    End Property

    Public ReadOnly Property [varDataType](ByVal key As String) As String
        Get
            Dim ret As String = Nothing
            Dim o As Object = Me.Vars(key)
            If Not (o Is Nothing) Then
                Try
                    ret = CStr(CType(o, String())(appVarCollection.eVarValueField.DataType))
                Catch ex As Exception
                    ret = Nothing
                End Try
            End If
            Return (ret)
        End Get
    End Property

    Public ReadOnly Property [varValue](ByVal key As String) As String
        Get
            Dim ret As String = Nothing
            Dim o As Object = Me.Vars(key)
            If Not (o Is Nothing) Then
                Try
                    ret = CStr(CType(o, String())(appVarCollection.eVarValueField.Value))
                Catch ex As Exception
                    ret = Nothing
                End Try
            End If
            Return (ret)
        End Get
    End Property

End Class
