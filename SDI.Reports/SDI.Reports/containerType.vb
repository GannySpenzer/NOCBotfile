Public Class containerType

    Private m_id As String = ""
    Private m_desc As String = ""

    Public Sub New()

    End Sub

    Public Sub New(ByVal id As String, ByVal desc As String)
        m_id = id
        m_desc = desc
    End Sub

    Public Property [Id]() As String
        Get
            Return m_id
        End Get
        Set(ByVal Value As String)
            m_id = Value
        End Set
    End Property

    Public Property [Description]() As String
        Get
            Return m_desc
        End Get
        Set(ByVal Value As String)
            m_desc = Value
        End Set
    End Property

    '// will return a collection of containerType type of objects
    Public Shared Function GetContainerTypes(Optional ByVal pathOverride As String = "") As ArrayList
        Dim arr As New ArrayList

        Dim appDir As String = System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName
        If appDir.Length > 0 Then
            appDir = appDir.Substring(startIndex:=0, length:=appDir.Length - (appDir.Length - appDir.LastIndexOf(CType("\", Char))))
        Else
            appDir = ""
        End If
        'appDir &= "\"
        Dim cntrTypes As String = ""
        If appDir.Length > 0 Then
            cntrTypes = appDir & "\containerType.xml"
        Else
            cntrTypes = "containerType.xml"
        End If
        ' apply override path if specified
        If pathOverride.Trim.Length > 0 Then
            cntrTypes = pathOverride & "containerType.xml"
        End If

        'Dim ass As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly
        'Dim assName As System.Reflection.AssemblyName = ass.GetName
        'Dim dllName As String = assName.Name
        Dim xmlRdr As System.Xml.XmlTextReader = New System.Xml.XmlTextReader(cntrTypes)

        ' read the file
        xmlRdr.WhitespaceHandling = Xml.WhitespaceHandling.None

        Dim strKey As String = ""
        Dim strVal As String = ""
        Dim typ As containerType = Nothing

        While (xmlRdr.Read)
            Select Case xmlRdr.NodeType
                Case Xml.XmlNodeType.Element
                    If xmlRdr.HasAttributes And xmlRdr.Name.Trim.ToUpper = "ADD" Then
                        typ = Nothing
                        strKey = ""
                        Try
                            strKey = xmlRdr.GetAttribute("id").Trim
                        Catch ex As Exception
                        End Try
                        strVal = ""
                        Try
                            strVal = xmlRdr.GetAttribute("desc").Trim
                        Catch ex As Exception
                        End Try
                        If strKey.Length > 0 Then
                            typ = New containerType(strKey, strVal)
                            arr.Add(typ)
                        End If
                    End If
            End Select
        End While

        xmlRdr.Close()
        xmlRdr = Nothing

        ' default sorting
        If arr.Count > 0 Then
            arr.Sort(Comparer:=New containerTypeIdSorter(False))
        End If

        Return arr
    End Function

End Class

'// sorter for containerType class object collection
Public Class containerTypeIdSorter

    Implements IComparer

    Private m_bIsAsc As Boolean = True

    Public Sub New(Optional ByVal IsDescendingOrder As Boolean = False)
        m_bIsAsc = Not IsDescendingOrder
    End Sub

    Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare
        Dim ret As Integer = 0
        Try

            ' get value to compare from object
            Dim sortIdX As String = CType(x, containerType).Id.Trim.ToUpper
            Dim sortIdY As String = CType(y, containerType).Id.Trim.ToUpper

            ret = sortIdX.CompareTo(sortIdY)

            ' change if descending order
            If Not m_bIsAsc Then
                ret = ret * -1
            End If

        Catch ex As Exception
            ' returns equal
            ret = 0
        End Try
        Return ret
    End Function

End Class