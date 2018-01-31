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
    Private mSMailboxName As String
    Private mMessageBodyName As String
    Private mSourceFolderName As WellKnownFolderName
    Private mSUserName As String
    Private mSPassword As String
    Private m_sExchangeURL As String = "https://mail.sdi.com/EWS/Exchange.asmx"
    Private mSProcessedFolder As String
    Private m_sReviewedFolder As String
    Private m_ProcessedParentFolder As WellKnownFolderName
    Private Service As New ExchangeService(ExchangeVersion.Exchange2010)
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
    Private M_BODYFilter As String
    Private m_bAttachmentNameFilterEnabled As Boolean
    Private m_sAttachmentNameFilter As String
    Private m_bAttachmentSizeLargerEnabled As Boolean
    Private m_sAttachmentSizeLargerFilter As String
    Private m_bAttachmentSizeSmallEnabled As Boolean
    Private m_sAttachmentSizeSmallFilter As String
    Private m_sResponseEmailRecipients As String

    Private m_bPrintAttachment As Boolean
    Private m_NoAttachmentHandler As runInformation.enum_NoAttachmentHandler
    Private m_sTempDir As String = Environment.CurrentDirectory & "\TMP"

    Public clsLogger As LoggerClass

    Overridable Function ProcessItem(ByVal currentItem As Item, ByRef bSkipEmail As Boolean) As Boolean
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

        Dim service As New ExchangeService(ExchangeVersion.Exchange2010)
        Dim mailBoxToProcess As New Mailbox(MailboxName)
        'zzzzzzzzzzz
        ' Dim mailmessage As New MessageBody(MessageBodyName)
        Dim fidSource = New FolderId(SourceFolderName, mailBoxToProcess)
        'zzzzzzzzzzzzzzzzz
        'Dim messageid As New MessageBody(SourceFolderName, mailmessage)
        'zzzzzzzzzzzzzzzzz
        Dim fidProcessedParent = New FolderId(ProcessedParentFolder, mailBoxToProcess)
       
        Dim iv As ItemView = New ItemView(999) ' return first 999 pages
        Dim fv As FolderView = New FolderView(999)
        Dim moveToFolder As FindFoldersResults = Nothing

        Dim inboxItems As FindItemsResults(Of Item) = Nothing
        Dim bodyitems As FindItemsResults(Of Item) = Nothing
        Dim processedFolderID As FolderId
        Dim reviewedFolderID As FolderId
        Dim bFolderFound As Boolean = False
        Dim bReviewFolderFound As Boolean = False
        Dim iCount As Integer = 1
        Dim myAttachment As FileAttachment

        Dim mySearch As SearchFilter
        Dim colFilters As New SearchFilter.SearchFilterCollection(LogicalOperator.And)

        clsLogger.Log_Event("entering exchange baseclass ProcessMailbox")
        Try


            service.Credentials = New WebCredentials(UserName, Password)
            service.Url = New Uri(ExchangeUrl, UriKind.Absolute)
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
                        colFilters.Add(New SearchFilter.ContainsSubstring(ItemSchema.IsFromMe, SubjectFilter))
                    End If
                End If
                inboxItems = service.FindItems(fidSource, colFilters, iv)
                '   mySearch.SearchFilterCollection = colFilters
                'mySearch.SearchFilterCollection.IsLessThan.SearchFilterCollection.IsLessThan = New SearchFilter.IsLessThan(ItemSchema.DateTimeReceived, Now.AddDays(-1))
                ' InboxItems = service.FindItems(fidSource, mySearch, iv)
                clsLogger.Log_Event("Items in Inbox using lastrun filter:  " & inboxItems.Count & " items to be processed")
            End If
            Dim fred As String = " "
            'Dim fred As Microsoft.Exchange.WebServices.Data.MimeContent
            If ProcessedFolderEnabled Then
                'msgbox("not coded properly")
                Debug.Print(ProcessedParentFolder)
                moveToFolder = service.FindFolders(fidProcessedParent, fv)

                For Each tmpFolder As Microsoft.Exchange.WebServices.Data.Folder In moveToFolder.Folders

                    If tmpFolder.DisplayName = ProcessedFolder Then
                        tmpFolder.Load(PropertySet.FirstClassProperties)
                        processedFolderID = tmpFolder.Id
                        bFolderFound = True
                        If bFolderFound And bReviewFolderFound Then
                            Exit For
                        End If
                    End If
                    If tmpFolder.DisplayName = ReviewedFolder Then
                        tmpFolder.Load(PropertySet.FirstClassProperties)
                        reviewedFolderID = tmpFolder.Id
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
                    processedFolderID = newFolder.Id
                End If
                If Not bReviewFolderFound Then
                    Dim newFolder As New Microsoft.Exchange.WebServices.Data.Folder(service)

                    newFolder.DisplayName = ReviewedFolder
                    newFolder.Save(fidProcessedParent)
                    reviewedFolderID = newFolder.Id
                End If
                If FilterEmails Then
                    If SubjectFilterEnabled Then
                        colFilters.Clear()
                        colFilters.Add(New SearchFilter.ContainsSubstring(ItemSchema.Subject, SubjectFilter))
                    End If

                    inboxItems = service.FindItems(fidSource, colFilters, iv)
                    'inboxItems.
                    clsLogger.Log_Event("Items in Inbox filtered by subject (" & SubjectFilter & "):  " & inboxItems.Count & " items to be processed")
                Else
                    inboxItems = service.FindItems(fidSource, iv)
                    clsLogger.Log_Event("Items in Inbox with no filter:  " & inboxItems.Count & " items to be processed")
                End If

            End If
            ' Dim fred As String = " "
            If Not UseLastRunFilter And Not ProcessedFolderEnabled Then
                If FilterEmails Then
                    If SubjectFilterEnabled Then
                        colFilters.Clear()
                        '''' zzzzzzz

                        fred = ItemSchema.Body.ToString
                        colFilters.Add(New SearchFilter.ContainsSubstring(ItemSchema.Subject, SubjectFilter))
                        'ZZZZZZZZZ
                        colFilters.Add(New SearchFilter.ContainsSubstring(ItemSchema.Body, BodyFilter))
                    End If
                    inboxItems = service.FindItems(fidSource, colFilters, iv)

                    'bodyitems = service.FindItems(mailmessage, colFilters, iv)

                Else
                    inboxItems = service.FindItems(fidSource, iv)
                    'bodyitems = service.FindItems(mailmessage, iv)
                   
                End If
            End If
            'ZZZZZZZZ
            fred = ItemSchema.Body.ToString
            inboxItems = service.FindItems(fidSource, colFilters, iv)

            fred = inboxItems.Items.ToString

            'Dim TED As ITEM IN 

            'service.LoadPropertiesForItems(findResults, New PropertySet(ItemSchema.Body))
            ' colFilters.Add(New SearchFilter.ContainsSubstring(ItemSchema.Body, BodyFilter))
            ' bodyitems = service.FindItems(fidSource, colFilters, iv)

            'ZZZZZZZZZZZ
            'msgbox("entering exchange baseclass items to process = " & InboxItems.Count.ToString)

            Dim bProcessEmail As Boolean = True
            Dim bMarkAsProcessed As Boolean = False
            For Each myItem As Item In inboxItems.Items
                bProcessEmail = True
                ' Debug.Print(InboxItems.Items(0).Subject)
                Dim sLogSender As String = ""
                Dim sLogSubject As String = ""
                Dim bGetSenderAlternate As Boolean = False
                Try
                    sLogSubject = myItem.Subject
                Catch ex As Exception
                End Try
                Try
                    sLogSender = myItem.DisplayCc
                    If sLogSender.Trim.Length = 0 Then
                        bGetSenderAlternate = True
                    End If
                Catch ex As Exception
                    bGetSenderAlternate = True
                End Try
                If bGetSenderAlternate Then
                    Try
                        sLogSender = CType(myItem, Microsoft.Exchange.WebServices.Data.EmailMessage).Sender.Name
                    Catch ex2 As Exception
                    End Try
                End If
                Try
                    clsLogger.Log_Event("ExchangeBaseClass:ProcessMailbox Begin processing from=" & sLogSender & "; subject=" & sLogSubject)
                Catch ex As Exception
                    clsLogger.Log_Event("ExchangeBaseClass:ProcessMailbox Begin processing next message")
                End Try
                If FilterEmails Then
                    If SubjectFilterEnabled Then
                        If Not (InStr(myItem.Subject, SubjectFilter) > 0) Then
                            clsLogger.Log_Event("ExchangeBaseClass:ProcessMailbox does not pass SubjectFilter; set bProcessEmail to false;")
                            bProcessEmail = False
                        End If
                        '''''''''''ZZZZZZZZZZZZZZ
                        Dim view As New ItemView(50)
                        view.PropertySet = (New PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Subject, ItemSchema.DateTimeReceived))
                        'view.PropertySet = (New PropertySet(BasePropertySet.IdOnly, ItemSchema.Subject, ItemSchema.DateTimeReceived))

                        'EmailMessage.Bind(service, myItem.Id, New PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Attachments))
                        EmailMessage.Bind(service, myItem.Id, New PropertySet(BasePropertySet.FirstClassProperties, EmailMessageSchema.From, ItemSchema.Attachments))

                        Dim fred9 As EmailAddressCollection


                        'Dim strmess As New  

                        'EmailMessage.Bind(service, myItem.Id, New PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Attachments))

                        'myItem.Body
                        'bodyitems = service.FindItems(mailmessage, colFilters, iv)
                        'Dim D As String = myItem.Body
                        'Dim srtmessage As String = bodyitems.ToString()
                        ' here put in the code to
                        ' search the PS-isa_users_tbl by email address to get userid business_unit... take the first one
                        ' insert a new magic ticket record H/L
                        ' status of 'preOPEN'
                        ' user fieldops/PAM
                        ' dept fieldOPS
                        '
                    End If
                End If
                ' this is where you want to process the message

                Dim bSkipEmail As Boolean = False
                If bProcessEmail Then
                    bMarkAsProcessed = ProcessItem(myItem, bSkipEmail)

                End If
                If ProcessedFolderEnabled Then
                    If Not bSkipEmail Then
                        If bMarkAsProcessed And bProcessEmail Then
                            clsLogger.Log_Event("ExchangeBaseClass:ProcessMailbox item will move to Processed folder")
                            myItem.Move(processedFolderID)
                        Else
                            'Dim myEmailClass As New EmailClass.EmailClass("joe.rank@sdi.com", "", "Email Processor Error", _
                            '"MailBox: " & MailboxName & vbCrLf & _
                            '"Error processing email with subject " & myItem.Subject & vbCrLf & _
                            '"Received " & myItem.DateTimeReceived.ToString) ' joe.rank@sdi.com
                            'If Not myEmailClass.SendEmail() Then
                            'clsLogger.Log_Event("ERROR SENDING EMAIL")
                            'End If
                            'myEmailClass = Nothing
                            clsLogger.Log_Event("ExchangeBaseClass:ProcessMailbox item will move to InComplete folder; bMarkAsProcessed=" & _
                                                bMarkAsProcessed.ToString & " bProcessEmail=" & bProcessEmail.ToString)
                            myItem.Move(reviewedFolderID)
                        End If
                    Else
                        clsLogger.Log_Event("ExchangeBaseClass:ProcessMailbox bSkipEmail set to true")
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
    Public Function PrintFile(ByVal fileName As String, ByVal printerName As String, ByVal timeout As Integer) As Integer


        ' FileName = "C:\Documents and Settings\joe.rank\My Documents\Dev\Projects\ExcelReader\Receipts\Receipts for July 21.xlsx"
        '   PDFFile = "C:\Documents and Settings\joe.rank\My Documents\Dev\Projects\PDFReader\CYTEC\Text\Processed\CYTECPO20101102141117.txt"

        If printerName.Trim.Length = 0 Then
            printerName = (New System.Drawing.Printing.PrinterSettings).PrinterName
        End If

        Dim proc As New System.Diagnostics.Process

        Proc.EnableRaisingEvents = True
        Proc.StartInfo.FileName = fileName
        Proc.StartInfo.Arguments = Chr(34) + printerName + Chr(34)
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
    Public Property BodyFilter() As String
        Get
            BodyFilter = M_BODYFilter
        End Get
        Set(ByVal value As String)
            m_BODYFilter = value
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
    Public Property ExchangeUrl() As String
        Get
            ExchangeUrl = m_sExchangeURL
        End Get
        Set(ByVal value As String)
            m_sExchangeURL = value
        End Set
    End Property

    Public Property MailboxName() As String
        Get
            MailboxName = mSMailboxName
        End Get
        Set(ByVal value As String)
            mSMailboxName = value
        End Set
    End Property
    'zzzzzzzzzzz
    Public Property MessageBodyName() As String
        Get
            MessageBodyName = mMessageBodyName
        End Get
        Set(ByVal value As String)
            mMessageBodyName = value
        End Set
    End Property

    Public Property SourceFolderName() As WellKnownFolderName
        Get
            SourceFolderName = mSourceFolderName
        End Get
        Set(ByVal value As WellKnownFolderName)
            mSourceFolderName = value
        End Set
    End Property
    Public Property ProcessedFolder() As String
        Get
            ProcessedFolder = mSProcessedFolder
        End Get
        Set(ByVal value As String)
            mSProcessedFolder = value
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
            UserName = mSUserName
        End Get
        Set(ByVal value As String)
            mSUserName = value
        End Set
    End Property

    Public Property Password() As String
        Get
            ' Password = m_sPassword
            Dim clsPassword As New PasswordClass
            Return clsPassword.DecryptPassword(mSPassword)
        End Get
        Set(ByVal value As String)
            mSPassword = value
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





    Protected Overrides Sub Finalize()

        On Error Resume Next
        Dim tmpDir As Scripting.Folder
        Dim tmpFso As New Scripting.FileSystemObject

        tmpDir = tmpFso.GetFolder(TempDir)
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

    Public Property ResponseEmailRecipients() As String
        Get
            If m_sResponseEmailRecipients Is Nothing Then
                ResponseEmailRecipients = ""
            Else
                ResponseEmailRecipients = m_sResponseEmailRecipients
            End If
        End Get
        Set(value As String)
            m_sResponseEmailRecipients = value
        End Set
    End Property
End Class