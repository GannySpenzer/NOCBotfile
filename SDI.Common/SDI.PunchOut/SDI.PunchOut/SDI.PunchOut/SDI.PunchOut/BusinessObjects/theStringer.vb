Public Class theStringer

    Private m_baseString As String = ""
    Private m_params As Hashtable = Nothing

    Public Sub New()

    End Sub

    Public Sub New(ByVal base As String)
        m_baseString = base
    End Sub

    Public Sub New(ByVal base As String, ByVal paramNames As String(), ByVal paramValues As String())
        m_baseString = base
        If Not (paramNames Is Nothing) And Not (paramValues Is Nothing) Then
            For nCtr As Integer = 0 To (paramNames.Length - 1)
                Dim sKey As String = paramNames(nCtr)
                Dim sValue As String = Nothing
                Try
                    sValue = paramValues(nCtr)
                Catch ex As Exception
                End Try
                Me.Parameters.Add(sKey, sValue)
            Next
        End If
    End Sub

    Public Overridable Property BaseString() As String
        Get
            Return m_baseString
        End Get
        Set(ByVal Value As String)
            m_baseString = Value
        End Set
    End Property

    Public Overridable ReadOnly Property Parameters() As Hashtable
        Get
            If (m_params Is Nothing) Then
                m_params = New Hashtable
            End If
            Return m_params
        End Get
    End Property

    Public Overrides Function [ToString]() As String
        Dim sql As String = m_baseString
        Dim sKey As String = ""
        Dim arrKey As New ArrayList
        For Each sKey In Me.Parameters.Keys
            arrKey.Add(sKey)
        Next
        If arrKey.Count > 0 Then
            ' so we replace the longest (with same name) param first to avoid partial replace
            '   e.g., ":INVOICE_DT" and ":INVOICE_DT_FORMAT"
            arrKey.Sort(Comparer:=New theStringerParamKeyComparer(IsDescendingLength:=True))
        End If
        For Each sKey In arrKey
            sql = sql.Replace(sKey, CStr(Me.Parameters(sKey)))
        Next
        arrKey = Nothing
        Return sql
    End Function

End Class


'//
'// compares the string based on their length
'//     can either be shortest to longest or vise versa (depends on m_bIsAsc value)
'//
Public Class theStringerParamKeyComparer

    Implements IComparer

    Private m_bIsAsc As Boolean = True

    Public Sub New(Optional ByVal IsDescendingLength As Boolean = False)
        m_bIsAsc = Not IsDescendingLength
    End Sub

    Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare
        Dim ret As Integer = 0
        Dim itmX As String = CStr(x).Trim
        Dim itmY As String = CStr(y).Trim
        'ret = itmX.CompareTo(itmY)
        ret = itmX.Length.CompareTo(itmY.Length)
        If Not m_bIsAsc Then
            ret = ret * -1
        End If
        Return ret
    End Function

End Class