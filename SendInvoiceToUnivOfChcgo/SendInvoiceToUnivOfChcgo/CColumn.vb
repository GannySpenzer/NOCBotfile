Public Class CColumn

    Inherits System.Data.DataColumn

    Private m_columnId As String = ""
    Private m_columnLabel As String = ""

    Private m_targetTableName As String = ""
    Private m_targetColumnName As String = ""
    Private m_targetValue As String = ""
    Private m_targetColumnLength As Integer = 0
    Private m_targetColumnDataType As String = ""
    Private m_targetColumnKey As Boolean = False

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(ByVal columnId As String, _
                   ByVal columnName As String, _
                   ByVal columnLabel As String, _
                   ByVal targetTableName As String, _
                   ByVal targetColumnName As String, _
                   ByVal targetColumnDataType As String, _
                   ByVal targetColumnLength As Integer, _
                   Optional ByVal targetValue As String = "", _
                   Optional ByVal targetColumnKey As Boolean = False)
        MyBase.New(columnName)
        m_columnId = columnId
        m_columnLabel = columnLabel
        m_targetTableName = targetTableName
        m_targetColumnName = targetColumnName
        m_targetColumnLength = targetColumnLength
        m_targetValue = targetValue
        m_targetColumnDataType = targetColumnDataType
        m_targetColumnKey = targetColumnKey
    End Sub

    Public Property ColumnId As String
        Get
            Return m_columnId
        End Get
        Set(value As String)
            m_columnId = value
        End Set
    End Property

    Public Property ColumnLabel As String
        Get
            Return m_columnLabel
        End Get
        Set(value As String)
            m_columnLabel = value
        End Set
    End Property

    Public Property [TargetTableName] As String
        Get
            Return m_targetTableName
        End Get
        Set(value As String)
            m_targetTableName = value
        End Set
    End Property

    Public Property [TargetColumnName] As String
        Get
            Return m_targetColumnName
        End Get
        Set(value As String)
            m_targetColumnName = value
        End Set
    End Property

    Public Property [TargetColumnDataType] As String
        Get
            Return m_targetColumnDataType
        End Get
        Set(value As String)
            m_targetColumnDataType = value
        End Set
    End Property

    Public Property [TargetColumnLength] As Integer
        Get
            Return m_targetColumnLength
        End Get
        Set(value As Integer)
            m_targetColumnLength = value
        End Set
    End Property

    Public Property [TargetValue] As String
        Get
            Return m_targetValue
        End Get
        Set(value As String)
            m_targetValue = value
        End Set
    End Property

    Public Property [TargetColumnKey] As Boolean
        Get
            Return m_targetColumnKey
        End Get
        Set(value As Boolean)
            m_targetColumnKey = value
        End Set
    End Property

End Class
