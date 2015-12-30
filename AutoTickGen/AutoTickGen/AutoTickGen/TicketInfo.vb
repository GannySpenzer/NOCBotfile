Imports System.Xml.Serialization

Public Class TicketInfo

    Private m_sAssigneeID As String
    Public Property AssigneeID As String
        Get
            AssigneeID = m_sAssigneeID
        End Get
        Set(value As String)
            m_sAssigneeID = value
            If m_sAssigneeID.Trim.Length > 0 Then
                m_oAssigneeInfo = New AssigneeInfo(m_sAssigneeID)
            End If
        End Set
    End Property

    Private m_oAssigneeInfo As AssigneeInfo
    Public Function AssigneeInfo() As AssigneeInfo
        Return m_oAssigneeInfo
    End Function

    Private m_sPriority As String
    Public Property Priority As String
        Get
            Priority = m_sPriority
        End Get
        Set(value As String)
            m_sPriority = value
        End Set
    End Property

    Private m_sRequestorID As String
    Public Property RequestorID As String
        Get
            RequestorID = m_sRequestorID
        End Get
        Set(value As String)
            m_sRequestorID = value
            If m_sRequestorID.Trim.Length > 0 Then
                m_oRequestorInfo = New UserInfo(m_sRequestorID)
            End If
        End Set
    End Property

    Private m_oRequestorInfo As UserInfo
    Public Function RequestorInfo() As UserInfo
        Return m_oRequestorInfo 'New UserInfo(RequestorID)
    End Function

    Private m_sTicketType As String
    Public Property TicketType As String
        Get
            TicketType = m_sTicketType
        End Get
        Set(value As String)
            m_sTicketType = value
        End Set
    End Property

    Private m_sEmailSubject As String
    Public Property EmailSubject As String
        Get
            EmailSubject = m_sEmailSubject
        End Get
        Set(value As String)
            m_sEmailSubject = value
        End Set
    End Property

    Private m_sIncludeWeekends As String
    Public Property IncludeWeekends As String
        Get
            IncludeWeekends = m_sIncludeWeekends
        End Get
        Set(value As String)
            m_sIncludeWeekends = value
        End Set
    End Property

    Private m_sFrequency As String = ""
    Public Property Frequency As String
        Get
            Frequency = m_sFrequency
        End Get
        Set(value As String)
            m_sFrequency = value
        End Set
    End Property

    Private m_sEmailCCs As String
    Public Property EmailCCs As String
        Get
            EmailCCs = m_sEmailCCs
        End Get
        Set(value As String)
            m_sEmailCCs = value
            ParseEmailCCs()
        End Set
    End Property

    Private m_sEmailCC As ArrayList
    Private Sub ParseEmailCCs()
        m_sEmailCC = New ArrayList
        Dim emails As String() = m_sEmailCCs.Split(";")
        For Each s As String In emails
            s = s.Trim
            If s.Length > 0 Then
                ' If the CC has the character "@", assume it's an email.
                ' If there is no "@", assume it's a user ID so look up the
                ' email from the Oracle users table.
                If s.IndexOf("@") >= 0 Then
                    m_sEmailCC.Add(s)
                Else
                    Dim oCCUser As New UserInfo(s)
                    If oCCUser.ExistsUser Then
                        m_sEmailCC.Add(oCCUser.Email)
                    End If
                End If
            End If
        Next
    End Sub

    Public ReadOnly Property EmailCCList As ArrayList
        Get
            Return m_sEmailCC
        End Get
    End Property

    Private m_oTasks As Object
    Public Property Tasks As Object
        Get
            Return m_oTasks
        End Get
        Set(value As Object)
            m_oTasks = value

            m_arrTasks = New ArrayList
            Dim sDescription As String = ""
            Dim i As Integer = 0
            While i < value.length
                Dim elem As System.Xml.XmlElement = CType(value(i), System.Xml.XmlElement)
                m_arrTasks.Add(elem.InnerText)

                If i = 0 Then
                    sDescription = elem.InnerText
                Else
                    sDescription &= ControlChars.NewLine & elem.InnerText
                End If

                i = i + 1
            End While

            ' For now, replace angle brackets with parentheses just to get this
            ' deployed. If we don't replace the angle brackets, they and the contents
            ' between them don't show up in the email.
            sDescription = sDescription.Replace("<", "(")
            sDescription = sDescription.Replace(">", ")")

            m_sDescription = sDescription

        End Set
    End Property

    ' This is used for logging the raw tasks read from the task file.
    Private m_arrTasks As ArrayList
    Public ReadOnly Property TaskArray As ArrayList
        Get
            Return m_arrTasks
        End Get
    End Property

    ' This is used as the formatted description for the ticket formed
    ' from the raw tasks read from the task file.
    Private m_sDescription As String
    Public Function Description() As String
        Return m_sDescription
    End Function

    ' NOTE: This Tasks property is currently commented out. It's an updated version of
    ' the Tasks property which has code that stores a URL with the delimiters
    ' to let a label control show the URL as a link. We're commenting this out now
    ' because SDiExchange crashes when trying to close the ticket with the URL like this.
    ' We need to figure out why SDiExchange crashes so, for now, we'll comment out
    ' this code -- which seems to work when following the link from SDiExchange's
    ' Ticket System but not, as already mentioned, when saving the ticket.
    'Private m_oTasks As Object
    'Public Property Tasks As Object
    '    Get
    '        Return m_oTasks
    '    End Get
    '    Set(value As Object)
    '        m_oTasks = value

    '        Const cStartURLDelim As String = "<![CDATA["
    '        Const cEndURLDelim As String = "]]"

    '        m_arrTasks = New ArrayList
    '        Dim sDescription As String = ""
    '        Dim i As Integer = 0
    '        Dim bHasURL As Boolean = False
    '        While i < value.length
    '            Dim elem As System.Xml.XmlElement = CType(value(i), System.Xml.XmlElement)
    '            m_arrTasks.Add(elem.InnerText)

    '            If i > 0 Then
    '                sDescription &= ControlChars.NewLine
    '            End If

    '            Dim iURLDelimStartPosition As Integer = elem.InnerXml.IndexOf(cStartURLDelim)
    '            If iURLDelimStartPosition >= 0 Then
    '                bHasURL = True

    '                ' get any text before the URL
    '                Dim iURLDelimEndPosition As Integer = elem.InnerXml.IndexOf(cEndURLDelim)
    '                If iURLDelimStartPosition > 0 Then
    '                    sDescription &= elem.InnerXml.Substring(0, iURLDelimStartPosition)
    '                End If

    '                ' get URL itself
    '                Dim iURLStartPos As Integer = iURLDelimStartPosition + cStartURLDelim.Length
    '                Dim iURLLength As Integer = iURLDelimEndPosition - iURLStartPos
    '                sDescription &= "<a href='" & elem.InnerXml.Substring(iURLStartPos, iURLLength) & "'>" & elem.InnerXml.Substring(iURLStartPos, iURLLength) & "</a>"

    '                ' get any text after the URL
    '                If iURLDelimEndPosition < (elem.InnerXml.Length - 1) Then
    '                    sDescription &= elem.InnerXml.Substring(iURLDelimEndPosition + cEndURLDelim.Length + 1)
    '                End If
    '            Else
    '                sDescription &= elem.InnerText
    '            End If


    '            i = i + 1
    '        End While

    '        ' For now, replace angle brackets with parentheses just to get this
    '        ' deployed. If we don't replace the angle brackets, they and the contents
    '        ' between them don't show up in the email.
    '        If Not bHasURL Then
    '            sDescription = sDescription.Replace("<", "(")
    '            sDescription = sDescription.Replace(">", ")")
    '        End If

    '        m_sDescription = sDescription

    '    End Set
    'End Property

End Class
