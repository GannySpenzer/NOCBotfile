Imports XMLSerialization
Public Class runInformation
    Inherits SerializableData
    Public Enum enum_DuplicateFileHandler
        NONE = 0
        APPEND = 1
        OVERWRITE = 2
        IGNORE = 3
    End Enum
    Public Enum enum_NoAttachmentHandler
        NONE = 0
        NONE_PROCESSED = 1
        NONE_REVIEW = 2
        PRINT_MSG = 3
    End Enum
    Private m_LastRunTime As DateTime

    Public Encrypted_Password As String

    Private m_Enabled As Boolean
    Private m_SaveEmailBody As Boolean
    Public RunName As String
    Public EmailAddress As String
    Public SourceFolder As String
    Public ProcessedFolderEnabled As Boolean
    Public ProcessedFolderParent As String
    Public ProcessedFolder As String
    Public ReviewedFolder As String
    Public UserName As String
    Private m_Password As String
    Private m_ExchangeURL As String
    Public SaveToFolderEnabled As Boolean
    Public SaveToFolder As String
    Public UseLastRunFilter As Boolean
    Public UseFileTypeFilter As Boolean
    Public FileTypes As String
    Public RenameFile As Boolean
    Public RenamingFormat As String
    Public RenamingDateFormat As String
    Public CreateDateAsMessageDate As Boolean
    Public RemoveRE As Boolean
    Public DuplicateFileHandler As enum_DuplicateFileHandler

    Public FilterEmails As Boolean
    Public SubjectFilterEnabled As Boolean
    Public SubjectFilter As String
    Public AttachmentNameFilterEnabled As Boolean
    Public AttachmentNameFilter As String
    Public AttachmentSizeLargerEnabled As Boolean
    Public AttachmentSizeLargerFilter As String
    Public AttachmentSizeSmallEnabled As Boolean
    Public AttachmentSizeSmallFilter As String
    Public PrintAttachments As Boolean
    Public NoAttachmentHandler As enum_NoAttachmentHandler

    Public Property Password() As String
        Get
            Password = m_Password
        End Get
        Set(ByVal value As String)
            m_Password = value
        End Set
    End Property

    Public Property ExchangeURL() As String
        Get
            ExchangeURL = m_ExchangeURL
        End Get
        Set(ByVal value As String)
            m_ExchangeURL = value
        End Set
    End Property
    Public Property LastRunTime() As DateTime
        Get
          
            LastRunTime = m_LastRunTime

        End Get
        Set(ByVal value As DateTime)

            m_LastRunTime = value

        End Set
    End Property

    Public Property Enabled() As Boolean
        Get
            Enabled = m_Enabled
        End Get
        Set(ByVal value As Boolean)
            m_Enabled = value
        End Set
    End Property
    Public Property SaveEmailBody() As Boolean
        Get
            SaveEmailBody = m_SaveEmailBody
        End Get
        Set(ByVal value As Boolean)
            m_SaveEmailBody = value
        End Set
    End Property

    
End Class
