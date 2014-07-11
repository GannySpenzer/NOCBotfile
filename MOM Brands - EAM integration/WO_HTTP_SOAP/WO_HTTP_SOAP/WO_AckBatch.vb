Public Class WO_AckBatch

    Private m_arrUniqueIds As Hashtable = Nothing
    Private m_ackColumnId As String = ""
    Private m_ackBatchSize As Integer = 0

    Public Sub New()
        MyBase.New()
    End Sub

    Public ReadOnly Property [UniqueIds] As Hashtable
        Get
            If (m_arrUniqueIds Is Nothing) Then
                m_arrUniqueIds = New Hashtable
            End If
            Return m_arrUniqueIds
        End Get
    End Property

    Public Property ColumnIdToSave As String
        Get
            Return m_ackColumnId
        End Get
        Set(value As String)
            m_ackColumnId = value
        End Set
    End Property

    Public Property AcknowledgementBatchSize As Integer
        Get
            Return m_ackBatchSize
        End Get
        Set(value As Integer)
            m_ackBatchSize = value
        End Set
    End Property

End Class
