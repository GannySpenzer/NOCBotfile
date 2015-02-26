Imports Microsoft.Exchange.WebServices.Data
Imports System.IO


Public Class GenericClass
    Inherits ExchangeBaseClass


    Public Sub New()
        'MailboxName = "joe.rank@sdi.com"
        'SourceFolderName = WellKnownFolderName.Inbox
        'ProcessedParentFolder = WellKnownFolderName.Inbox
        'ProcessedFolder = "Processed"
        'UserName = "joe.rank"
        'Password = "J0seph!!"
    End Sub
    Public Sub New(ByVal runInfo As runInformation)

        If Not String.IsNullOrEmpty(runInfo.ExchangeURL) Then
            ExchangeURL = runInfo.ExchangeURL
        End If
        MailboxName = runInfo.EmailAddress

        SourceFolderName = WellKnownFolderName.Inbox
        ProcessedFolderEnabled = runInfo.ProcessedFolderEnabled
        If ProcessedFolderEnabled Then
            Select Case runInfo.ProcessedFolderParent
                Case Is = "Inbox"
                    ProcessedParentFolder = WellKnownFolderName.Inbox
                Case Is = "Calendar"
                    ProcessedParentFolder = WellKnownFolderName.Calendar
                Case Else
                    ProcessedParentFolder = WellKnownFolderName.Inbox
            End Select

            ProcessedFolder = runInfo.ProcessedFolder
            ReviewedFolder = runInfo.ReviewedFolder
            '  UseLastRunFilter = False
        End If
        'insert last runtime
        LastRunTime = runInfo.LastRunTime
        UseLastRunFilter = runInfo.UseLastRunFilter


        UserName = runInfo.UserName
        Password = runInfo.Password
        SaveToFolderEnabled = runInfo.SaveToFolderEnabled
        SaveToFolder = runInfo.SaveToFolder
        If Not SaveToFolder.EndsWith("\") Then
            SaveToFolder &= "\"
        End If
        '  UseLastRunFilter = runInfo.UseLastRunFilter
        UseFileTypeFilter = runInfo.UseFileTypeFilter
        FileTypes = runInfo.FileTypes
        RenameFile = runInfo.RenameFile
        RenamingFormat = runInfo.RenamingFormat
        RenamingDateFormat = runInfo.RenamingDateFormat
        CreateDateAsMessageDate = runInfo.CreateDateAsMessageDate
        DuplicateFileHandler = runInfo.DuplicateFileHandler
        RemoveRE = runInfo.RemoveRE

        FilterEmails = runInfo.FilterEmails
        SubjectFilterEnabled = runInfo.SubjectFilterEnabled
        SubjectFilter = runInfo.SubjectFilter
        AttachmentNameFilterEnabled = runInfo.AttachmentNameFilterEnabled
        AttachmentNameFilter = runInfo.AttachmentNameFilter
        AttachmentSizeLargerEnabled = runInfo.AttachmentSizeLargerEnabled
        AttachmentSizeLargerFilter = runInfo.AttachmentSizeLargerFilter
        AttachmentSizeSmallEnabled = runInfo.AttachmentSizeSmallEnabled
        AttachmentSizeSmallFilter = runInfo.AttachmentSizeSmallFilter
        PrintAttachment = runInfo.PrintAttachments
        NoAttachmentHandler = runInfo.NoAttachmentHandler
        SaveEmailBody = runInfo.SaveEmailBody



    End Sub

    Overrides Function ProcessItem(ByVal currentItem As Item) As Boolean
        Dim bReturn As Boolean = True
        Dim sFileName As String
        Dim sCheckForFile As String
        Dim iCount As Integer
        Dim iPlaceNumber As Integer
        ' Dim sUniqueKey As String
        Dim dtMessageDate As DateTime
        Dim saFormats As String()

        Try

            'If SaveToFolderEnabled Then
            currentItem.Load(PropertySet.FirstClassProperties)
            dtMessageDate = currentItem.DateTimeReceived
            For Each myAttachment As FileAttachment In currentItem.Attachments
                Dim bProcess As Boolean = False
                myAttachment.Load()
                If UseFileTypeFilter Then
                    clsLogger.Log_Event("Using file filter to process attachments")
                    saFormats = FileTypes.Split(";")
                    For iCount = 0 To saFormats.Count - 1
                        Debug.Print(myAttachment.Name.Substring(myAttachment.Name.Length - 3))
                        If saFormats.Contains("*." & myAttachment.Name.Substring(myAttachment.Name.LastIndexOf(".") + 1)) Then
                            bProcess = True
                            clsLogger.Log_Event("File  " & myAttachment.Name.ToString & " as the extension " & myAttachment.Name.Substring(myAttachment.Name.LastIndexOf(".") + 1))

                            Exit For
                        End If
                    Next
                Else
                    bProcess = True
                End If

                If FilterEmails Then

                    If AttachmentNameFilterEnabled Then
                        If Not (InStr(myAttachment.Name, AttachmentNameFilter) > 0) Then
                            bProcess = False
                        End If
                    End If
                    If AttachmentSizeLargerEnabled And IsNumeric(AttachmentSizeLargerFilter) Then
                        If myAttachment.Content.Length < (Integer.Parse(AttachmentSizeLargerFilter) * 1000) Then
                            bProcess = False
                        End If
                    End If
                    If AttachmentSizeSmallEnabled And IsNumeric(AttachmentSizeSmallFilter) Then

                        If myAttachment.Content.Length > (Integer.Parse(AttachmentSizeSmallFilter) * 1000) Then
                            bProcess = False
                        End If
                    End If
                End If
                If bProcess Then
                    If SaveToFolderEnabled Then
                        sFileName = myAttachment.Name.Replace(" ", "_")
                        If RenameFile Then
                            'msgbox("Original File name : " & sFileName)
                            sFileName = FileNameChange(sFileName, currentItem)

                            sCheckForFile = sFileName
                            'msgbox("check fo File name : " & sFileName)

                        Else
                            sCheckForFile = sFileName

                        End If

                        Select Case DuplicateFileHandler
                            Case Is = runInformation.enum_DuplicateFileHandler.APPEND
                                iPlaceNumber = InStrRev(sFileName, ".")
                                sCheckForFile = SaveToFolder & sFileName
                                iCount = 1
                                Do While File.Exists(sCheckForFile)

                                    sCheckForFile = SaveToFolder & sFileName.Substring(0, iPlaceNumber - 1) & "_Dup_" & iCount & sFileName.Substring(iPlaceNumber - 1)
                                    ' sCheckForFile = sFileName.Substring(0, iPlaceNumber - 1) & "_" & iCount.ToString & sFileName.Substring(iPlaceNumber - 1)
                                    iCount += 1
                                Loop

                                myAttachment.Load(sCheckForFile)
                                If CreateDateAsMessageDate Then
                                    File.SetCreationTime(sCheckForFile, currentItem.DateTimeReceived)
                                End If
                            Case Is = runInformation.enum_DuplicateFileHandler.OVERWRITE
                                myAttachment.Load(sCheckForFile)
                                If CreateDateAsMessageDate Then
                                    File.SetCreationTime(sCheckForFile, currentItem.DateTimeReceived)
                                End If

                            Case Is = runInformation.enum_DuplicateFileHandler.IGNORE

                            Case Is = runInformation.enum_DuplicateFileHandler.NONE

                        End Select
                    End If
                    If PrintAttachment Then
                        Debug.Print("PRINT ATTACHMENT")
                        Debug.Print(myAttachment.Name)
                        Dim sFileExtension As String = ""

                        sFileExtension = myAttachment.Name.Substring(InStrRev(myAttachment.Name, "."))
                        Select Case LCase(sFileExtension)
                            Case Is = "pdf", "txt"
                                myAttachment.Load(TempDir & "\" & myAttachment.Name)
                                PrintFile(TempDir & "\" & myAttachment.Name, "", 60)
                            Case Is = "xls"
                                bProcess = False
                            Case Else
                                bProcess = False
                        End Select
                    End If
                    If Not bProcess Then
                        bReturn = False
                    End If
                End If

            Next
            'End If



        Catch ex As Exception
            clsLogger.Log_Event("GenericClass:Processing Item ERROR: " & ex.Message)
            bReturn = False
        End Try

        Return bReturn

    End Function



End Class
