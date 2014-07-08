Public Class CYCCNT_batch

    Private m_index As Integer = 1
    Private m_batchSize As Integer = 5000
    Private m_batchProcessedRecordCount As Integer = 0
    Private m_err As Exception = Nothing

    Public Sub New()
        MyBase.New()
    End Sub

    Public Property [index] As Integer
        Get
            Return m_index
        End Get
        Set(value As Integer)
            m_index = value
        End Set
    End Property

    Public Property [BatchSize] As Integer
        Get
            Return m_batchSize
        End Get
        Set(value As Integer)
            m_batchSize = value
        End Set
    End Property

    Public Property [ex] As Exception
        Get
            Return m_err
        End Get
        Set(value As Exception)
            m_err = value
        End Set
    End Property

    Public Property [BatchProcessedRecordCount] As Integer
        Get
            Return m_batchProcessedRecordCount
        End Get
        Set(value As Integer)
            m_batchProcessedRecordCount = value
        End Set
    End Property

End Class
