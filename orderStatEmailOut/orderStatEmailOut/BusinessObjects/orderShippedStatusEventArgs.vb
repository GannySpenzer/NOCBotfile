Imports System.Data
Imports System.Data.OleDb
Imports SDI.ApplicationLogger


Public Class orderShippedStatusEventArgs

    Inherits System.EventArgs

    Public Sub New()
        MyBase.New()
    End Sub

    Private m_oledbCN As OleDbConnection = Nothing

    Public Property [oledbCN]() As OleDbConnection
        Get
            Return m_oledbCN
        End Get
        Set(ByVal Value As OleDbConnection)
            m_oledbCN = Value
        End Set
    End Property

    Private m_logger As IApplicationLogger = Nothing

    Public Property [Logger]() As IApplicationLogger
        Get
            Return m_logger
        End Get
        Set(ByVal Value As IApplicationLogger)
            m_logger = Value
        End Set
    End Property

    Private m_arr As New Hashtable

    Public Property [AppSettings]() As Hashtable
        Get
            Return m_arr
        End Get
        Set(ByVal Value As Hashtable)
            m_arr = Value
        End Set
    End Property

End Class
