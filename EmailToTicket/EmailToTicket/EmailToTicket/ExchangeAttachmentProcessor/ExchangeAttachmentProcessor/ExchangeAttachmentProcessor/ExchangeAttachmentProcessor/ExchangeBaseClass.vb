'Imports System
'Imports System.IO
'Imports System.Xml
'Imports System.Collections.Generic
'Imports System.Text
'Imports System.Net
'Imports System.Net.Security
'Imports System.Security.Cryptography.X509Certificates
'Imports System.Security.Authentication
'Imports System.Text.RegularExpressions
Imports Microsoft.Exchange.WebServices.Data
Imports Scripting


Public Class ExchangeBaseClass
    Private m_sMailboxName As String
    Private m_SourceFolderName As WellKnownFolderName
    Private m_sUserName As String
    Private m_sPassword As String
    Private m_sExchangeURL As String = "https://mail.sdi.com/EWS/Exchange.asmx"
    Private m_sProcessedFolder As String
    Private m_sReviewedFolder As String
    Private m_ProcessedParentFolder As WellKnownFolderName
    Private Service As New ExchangeService(ExchangeVersion.Exchange2007_SP1)
    Private m_mailbox As Mailbox
    Private m_fidSourceFolder As FolderId
    Private m_bProcessedFolderEnabled As Boolean
    Private m_fidProcessedParent As FolderId
    Private m_lastRunTime As DateTime
    Private m_UseLastRunFilter As Boolean
    Private m_bSaveAttachToFolder As Boolean
    Private m_sSaveToFolder As String

    Private m_bUseFileTypeFilter As Boolean
    Private m_sFileTypes As String
    Private m_bRenameFile As Boolean
    Private m_sRenamingFormat As String
    Private m_sRenamingDateFormat As String
    Private m_bCreateDateAsMessageDate As Boolean
    Private m_bRemoveRE As Boolean
    Private m_DuplicateHandler As runInformation.enum_DuplicateFileHandler

    Private m_bFilterEmails As Boolean
    Private m_bSubjectFilterEnabled As Boolean
    Private m_sSubjectFilter As String
    Private m_bAttachmentNameFilterEnabled As Boolean
    Private m_sAttachmentNameFilter As String
    Private m_bAttachmentSizeLargerEnabled As Boolean
    Private m_sAttachmentSizeLargerFilter As String
    Private m_bAttachmentSizeSmallEnabled As Boolean
    Private m_sAttachmentSizeSmallFilter As String

    Private m_bPrintAttachment As Boolean
    Private m_NoAttachmentHandler As runInformation.enum_NoAttachmentHandler
    Private m_sTempDir As String = Environment.CurrentDirectory & "\TMP"

    Private m_bSaveEmailBody As Boolean
    Public clsLogger As LoggerClass

    Overridable Function ProcessItem(ByVal currentItem As Item) As Boolean
        Dim bReturn As Boolean = False
        Debug.Print(currentItem.Subject)
        currentItem.Load(PropertySet.FirstClassProperties)

        Debug.Print(currentItem.Attachments.Count)

        'For Each myAttachment In myItem.Attachments
        '    myAttachment.Load()
        '    Debug.Print(myAttachment.Name)
        'Next
        Return bReturn
    End Function

    Public Function ProcessMailbox(Optional ByVal dttmNow As DateTime = Nothing) As Boolean

        Dim bReturn As Boolean = True

        Dim service As New ExchangeService(ExchangeVersion.Exchange2007_SP1)
        Dim MailBoxToProcess As New Mailbox(MailboxName)
        Dim fidSource = New FolderId(SourceFolderName, MailBoxToProcess)
        Dim fidProcessedParent = New FolderId(ProcessedParentFolder, MailBoxToProcess)
        Dim iv As ItemView = New ItemView(999) ' return first 999 pages
        Dim fv As FolderView = New FolderView(999)
        Dim MoveToFolder As FindFoldersResults = Nothing

        Dim InboxItems As FindItemsResults(Of Item) = Nothing
        Dim ProcessedFolderID As FolderId
        Dim ReviewedFolderID As FolderId
        Dim bFolderFound As Boolean = False
        Dim bReviewFolderFound As Boolean = False
        Dim iCount As Integer = 1
        Dim myAttachment As FileAttachment

        Dim mySearch As SearchFilter
        Dim colFilters As New SearchFilter.SearchFilterCollection(LogicalOperator.And)

        clsLogger.Log_Event("entering exchange baseclass ProcessMailbox")
        Try



            service.Credentials = New WebCredentials(UserName, Password)
            service.Url = New Uri(ExchangeURL, UriKind.Absolute)
            iv.Traversal = ItemTraversal.Shallow

            If UseLastRunFilter Then
                ' colFilters.Add(New SearchFilter.IsGreaterThan(ItemSchema.DateTimeReceived, LastRunTime))
                colFilters.Add(New SearchFilter.IsGreaterThan(ItemSchema.DateTimeReceived, LastRunTime))
                If Not (IsNothing(dttmNow)) Then
                    colFilters.Add(New SearchFilter.IsLessThan(ItemSchema.DateTimeReceived, dttmNow))
                End If
                If FilterEmails Then
                    If SubjectFilterEnabled Then
                        colFilters.Add(New SearchFilter.ContainsSubstring(ItemSchema.Subject, SubjectFilter))
                    End If
                End If
                InboxItems = service.FindItems(fidSource, colFilters, iv)
                '   mySearch.SearchFilterCollection = colFilters
                'mySearch.SearchFilterCollection.IsLessThan.SearchFilterCollection.IsLessThan = New SearchFilter.IsLessThan(ItemSchema.DateTimeReceived, Now.AddDays(-1))
                ' InboxItems = service.FindItems(fidSource, mySearch, iv)
                clsLogger.Log_Event("Items in Inbox using lastrun filter:  " & InboxItems.Count & " items to be processed")
            End If
            If ProcessedFolderEnabled Then
                'msgbox("not coded properly")
                Debug.Print(ProcessedParentFolder)
                MoveToFolder = service.FindFolders(fidProcessedParent, fv)

                For Each tmpFolder As Microsoft.Exchange.WebServices.Data.Folder In MoveToFolder.Folders

                    If tmpFolder.DisplayName = ProcessedFolder Then
                        tmpFolder.Load(PropertySet.FirstClassProperties)
                        ProcessedFolderID = tmpFolder.Id
                        bFolderFound = True
                        If bFolderFound And bReviewFolderFound Then
                            Exit For
                        End If
                    End If
                    If tmpFolder.DisplayName = ReviewedFolder Then
                        tmpFolder.Load(PropertySet.FirstClassProperties)
                        ReviewedFolderID = tmpFolder.Id
                        bReviewFolderFound = True
                        If bFolderFound And bReviewFolderFound Then
                            Exit For
                        End If
                    End If
                Next

                If Not bFolderFound Then
                    Dim newFolder As New Microsoft.Exchange.WebServices.Data.Folder(service)

                    newFolder.DisplayName = ProcessedFolder
                    newFolder.Save(fidProcessedParent)
                    ProcessedFolderID = newFolder.Id
                End If
                If Not bReviewFolderFound Then
                    Dim newFolder As New Microsoft.Exchange.WebServices.Data.Folder(service)

                    newFolder.DisplayName = ReviewedFolder
                    newFolder.Save(fidProcessedParent)
                    ReviewedFolderID = newFolder.Id
                End If
                If FilterEmails Then
                    If SubjectFilterEnabled Then
                        colFilters.Clear()
                        colFilters.Add(New SearchFilter.ContainsSubstring(ItemSchema.Subject, SubjectFilter))
                    End If

                    InboxItems = service.FindItems(fidSource, colFilters, iv)
                    clsLogger.Log_Event("Items in Inbox filtered by subject (" & SubjectFilter & "):  " & InboxItems.Count & " items to be processed")
                Else
                    InboxItems = service.FindItems(fidSource, iv)
                    clsLogger.Log_Event("Items in Inbox with no filter:  " & InboxItems.Count & " items to be processed")
                End If

            End If
                If Not UseLastRunFilter And Not ProcessedFolderEnabled Then
                    If FilterEmails Then
                        If SubjectFilterEnabled Then
                            colFilters.Clear()
                            colFilters.Add(New SearchFilter.ContainsSubstring(ItemSchema.Subject, SubjectFilter))
                        End If

                        InboxItems = service.FindItems(fidSource, colFilters, iv)
                    Else
                        InboxItems = service.FindItems(fidSource, iv)
                    End If
                End If
                'msgbox("entering exchange baseclass items to process = " & InboxItems.Count.ToString)
                Dim bProcessEmail As Boolean = True
                Dim bMarkAsProcessed As Boolean = False
                For Each myItem As Item In InboxItems.Items
                    bProcessEmail = True
                    ' Debug.Print(InboxItems.Items(0).Subject)
                    If FilterEmails Then
                        If SubjectFilterEnabled Then
                            If Not (InStr(myItem.Subject, SubjectFilter) > 0) Then
                                bProcessEmail = False
                            End If
                        End If
                End If
                If SaveEmailBody Then
                    Try

                        clsLogger.Log_Event("Saving email body to text file")
                        Dim tmpFSO As New FileSystemObject
                        Dim tmpStream As TextStream
                        Dim tmpFileName As String = SaveToFolder & "ZapaOrder_Received_" & myItem.DateTimeReceived.ToString("yyyyMMdd_HHmm_") & Now.Millisecond & ".txt"
                        '                    Dim tmpFileName As String = SaveToFolder & "\" & myItem.Subject.Replace(" ", "_") & "_" & Now.Millisecond & ".txt"
                        Dim tmpPS As New PropertySet
                        tmpPS.Add(EmailMessageSchema.Body)
                        tmpStream = tmpFSO.CreateTextFile(tmpFileName)
                        myItem.Load(tmpPS)
                        tmpStream.Write(myItem.Body.Text)
                        tmpStream.Close()
                        tmpStream = Nothing
                        tmpFSO = Nothing
                        tmpPS = Nothing
                        bMarkAsProcessed = True
                    Catch ex As Exception
                        clsLogger.Log_Event("ERROR Saving body to textfile: " & ex.Message)
                    End Try

                ElseIf bProcessEmail Then
                    bMarkAsProcessed = ProcessItem(myItem)
                End If
                If ProcessedFolderEnabled Then
                    If bMarkAsProcessed And bProcessEmail Then
                        myItem.Move(ProcessedFolderID)
                    Else
                        Dim myEmailClass As New EmailClass.EmailClass("joe.rank@sdi.com", "", "Email Processor Error", _
                                                                       "MailBox: " & MailboxName & vbCrLf & _
                                                                       "Error processsing email with subject " & myItem.Subject & vbCrLf & _
                                                                       "Received " & myItem.DateTimeReceived.ToString)
                        If Not myEmailClass.SendEmail() Then
                            clsLogger.Log_Event("ERROR SENDING EMAIL")
                        End If
                        myEmailClass = Nothing
                        myItem.Move(ReviewedFolderID)
                    End If
                End If
            Next
        Catch ex As Exception
            Debug.Print(ex.Message)
            clsLogger.Log_Event("Process mailbox Error: " & ex.Message)
            bReturn = False
        End Try


        Return bReturn
    End Function

    Public Function FileNameChange(ByVal sFileName As String, ByVal myItem As Item) As String
        Dim sReturnString As String = ""
        Dim sWorkingName As String = ""

        Debug.Print(myItem.Subject)
        Debug.Print(RenamingFormat)
        sWorkingName = RenamingFormat
        If InStr(RenamingFormat, "%MessageDate%") > 0 Then
            Dim sTmpDateFormat As String = ""
            If RenamingDateFormat.Length = 0 Then
                sTmpDateFormat = "yyyyMMddHHmm"
            Else
                sTmpDateFormat = RenamingDateFormat
            End If

            sWorkingName = sWorkingName.Replace("%MessageDate%", myItem.DateTimeReceived.ToString(sTmpDateFormat)) '""))

        End If
        If InStr(RenamingFormat, "%FileName%") > 0 Then
            sWorkingName = sWorkingName.Replace("%FileName%", sFileName.Substring(0, sFileName.LastIndexOf(".")))
        End If
        If InStr(RenamingFormat, "%Subject%") > 0 Then
            Dim sSubject As String = ""


            If RemoveRE Then
                If InStr(myItem.Subject, "RE:") > 0 Or _
                   InStr(myItem.Subject, "FWD:") > 0 Or _
                   InStr(myItem.Subject, "FW:") > 0 Then
                    sSubject = myItem.Subject.Replace("FW:", "").Trim
                    sSubject = sSubject.Replace("FWD:", "").Trim
                    sSubject = sSubject.Replace("RE:", "").Trim
                Else
                    sSubject &= myItem.Subject
                End If

            Else
                sSubject &= myItem.Subject
            End If

            sWorkingName = sWorkingName.Replace("%Subject%", sSubject)
        End If

        If InStr(RenamingFormat, "%Extension%") > 0 Then
            Debug.Print(sFileName.Substring(sFileName.LastIndexOf(".") + 1))
            sWorkingName = sWorkingName.Replace("%Extension%", sFileName.Substring(sFileName.LastIndexOf(".") + 1))
        End If

        sReturnString = sWorkingName.Replace(" ", "_")


        Return sReturnString
    End Function
    Public Function PrintFile(ByVal FileName As String, ByVal PrinterName As String, ByVal Timeout As Integer) As Integer


        ' FileName = "C:\Documents and Settings\joe.rank\My Documents\Dev\Projects\ExcelReader\Receipts\Receipts for July 21.xlsx"
        '   PDFFile = "C:\Documents and Settings\joe.rank\My Documents\Dev\Projects\PDFReader\CYTEC\Text\Processed\CYTECPO20101102141117.txt"

        If PrinterName.Trim.Length = 0 Then
            PrinterName = (New System.Drawing.Printing.PrinterSettings).PrinterName
        End If

        Dim Proc As New System.Diagnostics.Process

        Proc.EnableRaisingEvents = True
        Proc.StartInfo.FileName = FileName
        Proc.StartInfo.Arguments = Chr(34) + PrinterName + Chr(34)
        Proc.StartInfo.Verb = "PrintTo"
        Proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized
        Proc.StartInfo.CreateNoWindow = True
        Proc.Start()

        Do While Timeout > 0 AndAlso Not Proc.HasExited
            System.Threading.Thread.Sleep(1000)
            Timeout -= 1
        Loop

        If Not Proc.HasExited Then
            Debug.Print("Killing process")
            Proc.Kill()
        End If

        Debug.WriteLine("Closing process")
        Proc.Close()

        Return 0
    End Function
  

    Public Property TempDir() As String
        Get
            TempDir = m_sTempDir
        End Get
        Set(ByVal value As String)
            m_sTempDir = value
        End Set
    End Property
    Public Property PrintAttachment() As Boolean
        Get
            PrintAttachment = m_bPrintAttachment
        End Get
        Set(ByVal value As Boolean)
            m_bPrintAttachment = value
        End Set
    End Property
    Public Property NoAttachmentHandler() As runInformation.enum_NoAttachmentHandler
        Get
            NoAttachmentHandler = m_NoAttachmentHandler
        End Get
        Set(ByVal value As runInformation.enum_NoAttachmentHandler)
            m_NoAttachmentHandler = value
        End Set
    End Property

    Public Property DuplicateFileHandler() As runInformation.enum_DuplicateFileHandler
        Get
            DuplicateFileHandler = m_DuplicateHandler
        End Get
        Set(ByVal value As runInformation.enum_DuplicateFileHandler)
            m_DuplicateHandler = value
        End Set
    End Property

    Public Property FilterEmails() As Boolean
        Get
            FilterEmails = m_bFilterEmails
        End Get
        Set(ByVal value As Boolean)
            m_bFilterEmails = value
        End Set
    End Property

    Public Property ProcessedFolderEnabled() As Boolean
        Get
            ProcessedFolderEnabled = m_bProcessedFolderEnabled
        End Get
        Set(ByVal value As Boolean)
            m_bProcessedFolderEnabled = value
        End Set
    End Property

    Public Property SubjectFilterEnabled() As Boolean
        Get
            SubjectFilterEnabled = m_bSubjectFilterEnabled
        End Get
        Set(ByVal value As Boolean)
            m_bSubjectFilterEnabled = value
        End Set
    End Property

  
    Public Property SubjectFilter() As String
        Get
            SubjectFilter = m_sSubjectFilter
        End Get
        Set(ByVal value As String)
            m_sSubjectFilter = value
        End Set
    End Property

    Public Property AttachmentNameFilterEnabled() As Boolean
        Get
            AttachmentNameFilterEnabled = m_bAttachmentNameFilterEnabled
        End Get
        Set(ByVal value As Boolean)
            m_bAttachmentNameFilterEnabled = value
        End Set
    End Property


    Public Property AttachmentNameFilter() As String
        Get
            AttachmentNameFilter = m_sAttachmentNameFilter
        End Get
        Set(ByVal value As String)
            m_sAttachmentNameFilter = value
        End Set
    End Property

    Public Property AttachmentSizeLargerEnabled() As Boolean
        Get
            AttachmentSizeLargerEnabled = m_bAttachmentSizeLargerEnabled
        End Get
        Set(ByVal value As Boolean)
            m_bAttachmentSizeLargerEnabled = value
        End Set
    End Property


    Public Property AttachmentSizeLargerFilter() As String
        Get
            AttachmentSizeLargerFilter = m_sAttachmentSizeLargerFilter
        End Get
        Set(ByVal value As String)
            m_sAttachmentSizeLargerFilter = value
        End Set
    End Property

    Public Property AttachmentSizeSmallEnabled() As Boolean
        Get
            AttachmentSizeSmallEnabled = m_bAttachmentSizeSmallEnabled
        End Get
        Set(ByVal value As Boolean)
            m_bAttachmentSizeSmallEnabled = value
        End Set
    End Property


    Public Property AttachmentSizeSmallFilter() As String
        Get
            AttachmentSizeSmallFilter = m_sAttachmentSizeSmallFilter
        End Get
        Set(ByVal value As String)
            m_sAttachmentSizeSmallFilter = value
        End Set
    End Property


    Public Property RemoveRE() As Boolean
        Get
            RemoveRE = m_bRemoveRE
        End Get
        Set(ByVal value As Boolean)
            m_bRemoveRE = value
        End Set
    End Property


    Public Property SaveToFolderEnabled() As Boolean
        Get
            SaveToFolderEnabled = m_bSaveAttachToFolder
        End Get
        Set(ByVal value As Boolean)
            m_bSaveAttachToFolder = value
        End Set
    End Property
    Public Property SaveToFolder() As String
        Get
            SaveToFolder = m_sSaveToFolder
        End Get
        Set(ByVal value As String)
            m_sSaveToFolder = value
        End Set
    End Property
    Public Property ExchangeURL() As String
        Get
            ExchangeURL = m_sExchangeURL
        End Get
        Set(ByVal value As String)
            m_sExchangeURL = value
        End Set
    End Property

    Public Property MailboxName() As String
        Get
            MailboxName = m_sMailboxName
        End Get
        Set(ByVal value As String)
            m_sMailboxName = value
        End Set
    End Property

    Public Property SourceFolderName() As WellKnownFolderName
        Get
            SourceFolderName = m_SourceFolderName
        End Get
        Set(ByVal value As WellKnownFolderName)
            m_SourceFolderName = value
        End Set
    End Property
    Public Property ProcessedFolder() As String
        Get
            ProcessedFolder = m_sProcessedFolder
        End Get
        Set(ByVal value As String)
            m_sProcessedFolder = value
        End Set
    End Property

    Public Property ReviewedFolder() As String
        Get
            ReviewedFolder = m_sReviewedFolder
        End Get
        Set(ByVal value As String)
            m_sReviewedFolder = value
        End Set
    End Property
    Public Property UserName() As String
        Get
            UserName = m_sUserName
        End Get
        Set(ByVal value As String)
            m_sUserName = value
        End Set
    End Property

    Public Property Password() As String
        Get
            ' Password = m_sPassword
            Dim clsPassword As New PasswordClass
            Return clsPassword.DecryptPassword(m_sPassword)
        End Get
        Set(ByVal value As String)
            m_sPassword = value
        End Set
    End Property
    Public Property ProcessedParentFolder() As WellKnownFolderName
        Get
            ProcessedParentFolder = m_ProcessedParentFolder
        End Get
        Set(ByVal value As WellKnownFolderName)
            m_ProcessedParentFolder = value
        End Set
    End Property
    Public Property LastRunTime() As DateTime
        Get
            LastRunTime = m_lastRunTime
        End Get
        Set(ByVal value As DateTime)
            m_lastRunTime = value
        End Set
    End Property

    Public Property UseLastRunFilter() As Boolean
        Get
            UseLastRunFilter = m_UseLastRunFilter
        End Get
        Set(ByVal value As Boolean)
            m_UseLastRunFilter = value
        End Set
    End Property

    Public Property UseFileTypeFilter() As Boolean
        Get
            UseFileTypeFilter = m_bUseFileTypeFilter
        End Get
        Set(ByVal value As Boolean)
            m_bUseFileTypeFilter = value
        End Set
    End Property

    Public Property FileTypes() As String
        Get
            FileTypes = m_sFileTypes
        End Get
        Set(ByVal value As String)
            m_sFileTypes = value
        End Set
    End Property

    Public Property RenameFile() As Boolean
        Get
            RenameFile = m_bRenameFile
        End Get
        Set(ByVal value As Boolean)
            m_bRenameFile = value
        End Set
    End Property

    Public Property RenamingFormat() As String
        Get
            RenamingFormat = m_sRenamingFormat
        End Get
        Set(ByVal value As String)
            m_sRenamingFormat = value
        End Set
    End Property
    Public Property RenamingDateFormat() As String
        Get
            RenamingDateFormat = m_sRenamingDateFormat
        End Get
        Set(ByVal value As String)
            m_sRenamingDateFormat = value
        End Set
    End Property
    Public Property CreateDateAsMessageDate() As Boolean
        Get
            CreateDateAsMessageDate = m_bCreateDateAsMessageDate
        End Get
        Set(ByVal value As Boolean)
            m_bCreateDateAsMessageDate = value
        End Set
    End Property

    Public Property SaveEmailBody() As Boolean
        Get
            SaveEmailBody = m_bSaveEmailBody
        End Get
        Set(ByVal value As Boolean)
            m_bSaveEmailBody = value
        End Set
    End Property





    Protected Overrides Sub Finalize()

        On Error Resume Next
        Dim tmpDir As Scripting.Folder
        Dim tmpFSO As New Scripting.FileSystemObject

        tmpDir = tmpFSO.GetFolder(TempDir)
        For Each tmpFile As Scripting.File In tmpDir.Files
            tmpFSO.DeleteFile(tmpFile.Path)
        Next

        Service = Nothing

        MyBase.Finalize()

    End Sub

    Public Sub New()
        If Not (System.IO.Directory.Exists(TempDir)) Then
            System.IO.Directory.CreateDirectory(TempDir)
        End If
    End Sub
End Class
