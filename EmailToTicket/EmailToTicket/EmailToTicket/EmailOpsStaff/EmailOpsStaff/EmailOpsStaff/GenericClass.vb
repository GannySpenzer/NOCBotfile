Imports Microsoft.Exchange.WebServices.Data
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Data.OleDb
Imports System.Data.SqlClient

Imports EmailOpsStaff.SQLDBAccess
Imports EmailOpsStaff.SQLDBData
Imports System.Xml
 Imports System.Web.Mail
Imports System.Web.UI
Imports System.Text
Imports UpdEmailOut

 
Public Class GenericClass
    Inherits ExchangeBaseClass
    Dim connectOR As New OleDbConnection("Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=prod")
    Dim connectSQL As New SqlClient.SqlConnection("server=DAZZLE;uid=sa;pwd=sdiadmin;initial catalog='pubs'")
    'Dim connectSQL As New SqlClient.SqlConnection("server=192.168.253.16;uid=sa;pwd=sdiadmin;initial catalog='pubs'")
    Dim objStreamWriter As StreamWriter
    'server=192.168.253.16;uid=sa;pwd=sdiadmin;initial catalog=pubs;
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
        ResponseEmailRecipients = runInfo.ResponseEmailRecipients
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
        'Dim fred As String = runInfo.DuplicateFileHandler
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



    End Sub

    Overrides Function ProcessItem(ByVal currentItem As Item, ByRef bSkipEmail As Boolean) As Boolean
        Dim bReturn As Boolean = True
        Dim iLine As Integer = 0
        Dim sFileName As String
        Dim sCheckForFile As String
        Dim iCount As Integer
        Dim iPlaceNumber As Integer
        ' Dim sUniqueKey As String
        Dim dtMessageDate As DateTime
        Dim saFormats As String()
        Dim inboxItems As FindItemsResults(Of Item) = Nothing
        'Dim strKey As String = currentItem.Id.ChangeKey.ToString\
        Dim strKey As String = currentItem.Id.ToString
        Dim strcc As String = " "
        Dim bLogBodyError As Boolean = False
        Dim sItemEmailAddress As String = " "

        Try
            iLine = 1
            strcc = FindSDIEmail(currentItem, sItemEmailAddress, bSkipEmail)
            iLine = 2
            'strcc = currentItem.DisplayCc.ToString
            'Dim tt As String = Microsoft.Exchange.WebServices.Data.ExchangeService.
            If strcc = "" And Not bSkipEmail Then
                clsLogger.Log_Event("EmailOpsStaff:GenericClass:Processing Item ERROR: No SDI email address; sItemEmailAddress=" & sItemEmailAddress)
                SendEmail(currentItem)
                bReturn = False
                Return bReturn
            End If

        Catch ex As Exception
            clsLogger.Log_Event("EmailOpsStaff:GenericClass:Processing Item ERROR: iLine=" & iLine.ToString & ex.Message)
            SendEmail(currentItem)
            bReturn = False
            Return bReturn
        End Try

        ''zzzzzzzzzzz
        'Dim mailmessage As New MessageBody(MessageBodyName)
        'Dim fidSource = New FolderId(SourceFolderName, mailBoxToProcess)
        ''zzzzzzzzzzzzzzzzz
        'Dim messageid As New MessageBody(SourceFolderName, mailmessage)
        ''zzzzzzzzzzzzzzzzz
        Try
            If Not bSkipEmail Then
                'If SaveToFolderEnabled Then
                'zzzzzzzzz
                Dim itemPropertySet As PropertySet
                iLine = 3
                itemPropertySet = New PropertySet(BasePropertySet.FirstClassProperties)
                iLine = 4
                itemPropertySet.RequestedBodyType = BodyType.Text
                iLine = 5
                currentItem.Load(itemPropertySet)
                iLine = 6

                If currentItem.Body.Text Is Nothing Then
                    iLine = 7
                    bLogBodyError = True
                ElseIf currentItem.Body.Text.Trim().Length = 0 Then
                    iLine = 8
                    bLogBodyError = True
                End If
                If bLogBodyError Then
                    clsLogger.Log_Event("EmailOpsStaff:GenericClass:Processing Item ERROR: Email body is empty; iLine=" & iLine.ToString)
                    SendEmail(currentItem)
                    bReturn = False
                    Return bReturn
                End If

                'currentItem.Load(PropertySet.FirstClassProperties)
                dtMessageDate = currentItem.DateTimeReceived
                'For Each myMessage As Mesagebody In currentItem.Attachments

                'Next
                Dim strMessagebody As String = " "
                Dim stremailSndr As String = " "
                Dim returnValue As EmailMessage
                iLine = 9
                Dim Service As New ExchangeService(ExchangeVersion.Exchange2007_SP1)
                iLine = 10
                Service.Credentials = New WebCredentials(UserName, Password)
                iLine = 11
                Service.Url = New Uri(ExchangeUrl, UriKind.Absolute)
                iLine = 12

                Dim propertySet1 As New PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Id)
                iLine = 13

                returnValue = EmailMessage.Bind(Service, strKey, propertySet1)
                iLine = 14

                'strip the html crap off of the message body in the email

                'For Each mybody As MessageBody In currentItem.Body.Text
                '    strMessagebody = strMessagebody + mybody.ToString
                'Next
                'strMessagebody = RemoveHTML(strMessagebody)
                'strMessagebody = Regex.Replace(strMessagebody, "<o:p>", " ")
                'strMessagebody = Regex.Replace(strMessagebody, "&nbsp;", " ")
                'strMessagebody = Regex.Replace(strMessagebody, "</o:p>", " ")
                strMessagebody = currentItem.Body.Text.Trim()
                iLine = 15

                Dim myemailSender As String = " "
                myemailSender = strcc
                iLine = 16
                myemailSender = Regex.Replace(myemailSender, " ", "")
                iLine = 17
                If connectOR.State <> ConnectionState.Open Then
                    iLine = 170
                    connectOR.Open()
                End If
                iLine = 18
                Dim objUserTbl As clsUserTbl
                iLine = 19
                Dim bSuccess As Boolean = True
                objUserTbl = New clsUserTbl(myemailSender, " ", connectOR, clsLogger, bSuccess, sItemEmailAddress)
                iLine = 20


                If Not bSuccess Then
                    bReturn = False
                    Return bReturn
                End If

                Dim strCustName As String = objUserTbl.EmployeeName
                iLine = 21
                Dim strCustEmail As String = objUserTbl.EmployeeEmail
                iLine = 22
                Dim strCustPhone As String = objUserTbl.PhoneNum
                iLine = 23
                Dim strreqsitenam As String = objUserTbl.SiteName
                iLine = 24
                Dim struserssitename As String = objUserTbl.SiteName
                iLine = 25
                Dim strreqbu As String = objUserTbl.Business_unit
                iLine = 26
                Dim strusersbu As String = objUserTbl.Business_unit
                iLine = 27
                Dim strusersemail As String = objUserTbl.EmployeeEmail
                iLine = 28
                Dim strIncText As String = strMessagebody
                iLine = 29
                Dim strRequestor As String = objUserTbl.EmployeeID
                iLine = 30
                Dim struser As String = objUserTbl.EmployeeID
                iLine = 31
                Dim strmastersitename As String = objUserTbl.SiteName
                iLine = 32
                Dim strmasteremail As String = objUserTbl.EmployeeEmail
                iLine = 33
                Dim strmasterbu As String = objUserTbl.Business_unit
                iLine = 34
                Dim strMasterUser1 As String = objUserTbl.EmployeeID
                iLine = 35
                Dim strMasterphone As String = objUserTbl.PhoneNum
                iLine = 36
                Dim strMasterUser As String = objUserTbl.EmployeeID
                iLine = 37
                connectOR.Close()
                'Dim objTicket As New ClsInsertTicket(parm)
                ' get a unique ticket number
                iLine = 38
                Dim intTickid As Integer = 0
                iLine = 380
                intTickid = clsNewTicNO(intTickid, connectSQL)
                iLine = 39
                Dim insertStatus As Integer = SaveState(intTickid, "Priority3", _
              struserssitename, strusersemail, _
              strCustEmail, _
              strreqbu, _
              strusersbu, _
              strreqsitenam, _
              strIncText, _
              strRequestor, _
              struser, _
              strCustPhone, _
              strCustPhone, _
              strmastersitename, _
              strmasteremail, _
              strmasterbu, _
              strMasterUser1, _
              strMasterphone, _
              strMasterUser, connectOR)
                iLine = 40
                Try
                    If insertStatus <> 1 Then
                        'insertStatus is the number of rows affected by the insert
                        clsLogger.Log_Event("EmailOpsStaff:GenericClass:Processing Item insertStatus=" & insertStatus.ToString)
                    End If
                Catch ex As Exception
                    clsLogger.Log_Event("EmailOpsStaff:GenericClass:Processing Item no ticket written iLine=" & iLine.ToString & " ERROR: " & ex.Message)
                    SendEmail(currentItem)
                    bReturn = False
                    Return bReturn
                End Try


                'Try
                '    If myemailSender Is Nothing Then
                '        myemailSender = " "
                '    End If
                'Catch ex As Exception
                '    myemailSender = " "
                'End Try
                ''Catch ex As Exception

                '' End Try



                For Each myAttachment As FileAttachment In currentItem.Attachments
                    Dim bProcess As Boolean = False
                    iLine = 41
                    myAttachment.Load()
                    iLine = 42
                    If UseFileTypeFilter Then
                        iLine = 43
                        clsLogger.Log_Event("Using file filter to process attachments")
                        iLine = 44
                        saFormats = FileTypes.Split(";")
                        iLine = 45
                        For iCount = 0 To saFormats.Count - 1
                            Debug.Print(myAttachment.Name.Substring(myAttachment.Name.Length - 3))
                            iLine = 46
                            If saFormats.Contains("*." & myAttachment.Name.Substring(myAttachment.Name.LastIndexOf(".") + 1)) Then
                                iLine = 47
                                bProcess = True
                                clsLogger.Log_Event("File  " & myAttachment.Name.ToString & " as the extension " & myAttachment.Name.Substring(myAttachment.Name.LastIndexOf(".") + 1))

                                Exit For
                            End If
                        Next
                    Else
                        bProcess = True
                    End If

                    If FilterEmails Then

                        iLine = 48
                        If AttachmentNameFilterEnabled Then
                            iLine = 49
                            If Not (InStr(myAttachment.Name, AttachmentNameFilter) > 0) Then
                                iLine = 50
                                bProcess = False
                            End If
                        End If
                        iLine = 51
                        If AttachmentSizeLargerEnabled And IsNumeric(AttachmentSizeLargerFilter) Then
                            iLine = 52
                            If myAttachment.Content.Length < (Integer.Parse(AttachmentSizeLargerFilter) * 1000) Then
                                iLine = 53
                                bProcess = False
                            End If
                        End If
                        iLine = 54
                        If AttachmentSizeSmallEnabled And IsNumeric(AttachmentSizeSmallFilter) Then
                            iLine = 55
                            If myAttachment.Content.Length > (Integer.Parse(AttachmentSizeSmallFilter) * 1000) Then
                                iLine = 56
                                bProcess = False
                            End If
                        End If
                    End If
                    iLine = 57
                    If bProcess Then
                        iLine = 58
                        If SaveToFolderEnabled Then
                            iLine = 59
                            sFileName = myAttachment.Name.Replace(" ", "_")
                            iLine = 60
                            If RenameFile Then
                                'msgbox("Original File name : " & sFileName)
                                iLine = 61
                                sFileName = FileNameChange(sFileName, currentItem)
                                iLine = 62

                                sCheckForFile = sFileName
                                'msgbox("check fo File name : " & sFileName)

                            Else
                                sCheckForFile = sFileName

                            End If

                            Select Case DuplicateFileHandler
                                Case Is = runInformation.enum_DuplicateFileHandler.APPEND
                                    iLine = 63
                                    iPlaceNumber = InStrRev(sFileName, ".")
                                    iLine = 64
                                    sCheckForFile = SaveToFolder & sFileName
                                    iLine = 65
                                    iCount = 1
                                    Do While File.Exists(sCheckForFile)
                                        iLine = 66

                                        sCheckForFile = SaveToFolder & sFileName.Substring(0, iPlaceNumber - 1) & "_Dup_" & iCount & sFileName.Substring(iPlaceNumber - 1)
                                        ' sCheckForFile = sFileName.Substring(0, iPlaceNumber - 1) & "_" & iCount.ToString & sFileName.Substring(iPlaceNumber - 1)
                                        iLine = 67
                                        iCount += 1
                                    Loop

                                    iLine = 68
                                    myAttachment.Load(sCheckForFile)
                                    iLine = 69
                                    If CreateDateAsMessageDate Then
                                        iLine = 70
                                        File.SetCreationTime(sCheckForFile, currentItem.DateTimeReceived)
                                        iLine = 71
                                    End If
                                Case Is = runInformation.enum_DuplicateFileHandler.OVERWRITE
                                    iLine = 72
                                    myAttachment.Load(sCheckForFile)
                                    iLine = 73
                                    If CreateDateAsMessageDate Then
                                        iLine = 74
                                        File.SetCreationTime(sCheckForFile, currentItem.DateTimeReceived)
                                        iLine = 75
                                    End If

                                Case Is = runInformation.enum_DuplicateFileHandler.IGNORE
                                    iLine = 76

                                Case Is = runInformation.enum_DuplicateFileHandler.NONE
                                    iLine = 77

                            End Select
                        End If
                        iLine = 78
                        If PrintAttachment Then
                            iLine = 79
                            Debug.Print("PRINT ATTACHMENT")
                            iLine = 80
                            Debug.Print(myAttachment.Name)
                            iLine = 81
                            Dim sFileExtension As String = ""

                            iLine = 82
                            sFileExtension = myAttachment.Name.Substring(InStrRev(myAttachment.Name, "."))
                            iLine = 83
                            Select Case LCase(sFileExtension)
                                Case Is = "pdf", "txt"
                                    iLine = 84
                                    myAttachment.Load(TempDir & "\" & myAttachment.Name)
                                    iLine = 85
                                    PrintFile(TempDir & "\" & myAttachment.Name, "", 60)
                                    iLine = 86
                                Case Is = "xls"
                                    iLine = 87
                                    bProcess = False
                                Case Else
                                    iLine = 88
                                    bProcess = False
                            End Select
                        End If
                        iLine = 89
                        If Not bProcess Then
                            iLine = 90
                            bReturn = False
                        End If
                    End If

                Next



                'End If

            End If

        Catch ex As Exception
            clsLogger.Log_Event("EmailOpsStaff:GenericClass:Processing Item iLine=" & iLine.ToString & " ERROR: " & ex.Message)
            bReturn = False
        End Try

        Return bReturn

    End Function

    Function RemoveHTML(ByVal strText)
        Dim TAGLIST As String
        TAGLIST = ";!--;!DOCTYPE;A;ACRONYM;ADDRESS;APPLET;AREA;B;BASE;BASEFONT;" & _
                  "BGSOUND;BIG;BLOCKQUOTE;BODY;BR;BUTTON;CAPTION;CENTER;CITE;CODE;" & _
                  "COL;COLGROUP;COMMENT;DD;DEL;DFN;DIR;DIV;DL;DT;EM;EMBED;FIELDSET;" & _
                  "FONT;FORM;FRAME;FRAMESET;HEAD;H1;H2;H3;H4;H5;H6;HR;HTML;I;IFRAME;IMG;" & _
                  "INPUT;INS;ISINDEX;KBD;LABEL;LAYER;LAGEND;LI;LINK;LISTING;MAP;MARQUEE;" & _
                  "MENU;META;NOBR;NOFRAMES;NOSCRIPT;OBJECT;OL;OPTION;P;PARAM;PLAINTEXT;" & _
                  "PRE;Q;S;SAMP;SCRIPT;SELECT;SMALL;SPAN;STRIKE;STRONG;STYLE;SUB;SUP;" & _
                  "TABLE;TBODY;TD;TEXTAREA;TFOOT;TH;THEAD;TITLE;TR;TT;U;UL;VAR;WBR;XMP;"

        Const BLOCKTAGLIST As String = ";APPLET;EMBED;FRAMESET;HEAD;NOFRAMES;NOSCRIPT;OBJECT;SCRIPT;STYLE;"

        Dim nPos1
        Dim nPos2
        Dim nPos3
        Dim strResult
        Dim strTagName
        Dim bRemove
        Dim bSearchForBlock

        nPos1 = InStr(strText, "<")
        Do While nPos1 > 0
            nPos2 = InStr(nPos1 + 1, strText, ">")
            If nPos2 > 0 Then
                strTagName = Mid(strText, nPos1 + 1, nPos2 - nPos1 - 1)
                strTagName = Replace(Replace(strTagName, vbCr, " "), vbLf, " ")

                nPos3 = InStr(strTagName, " ")
                If nPos3 > 0 Then
                    strTagName = Left(strTagName, nPos3 - 1)
                End If

                If Left(strTagName, 1) = "/" Then
                    strTagName = Mid(strTagName, 2)
                    bSearchForBlock = False
                Else
                    bSearchForBlock = True
                End If

                If InStr(1, TAGLIST, ";" & strTagName & ";", vbTextCompare) > 0 Then
                    bRemove = True
                    If bSearchForBlock Then
                        If InStr(1, BLOCKTAGLIST, ";" & strTagName & ";", vbTextCompare) > 0 Then
                            nPos2 = Len(strText)
                            nPos3 = InStr(nPos1 + 1, strText, "</" & strTagName, vbTextCompare)
                            If nPos3 > 0 Then
                                nPos3 = InStr(nPos3 + 1, strText, ">")
                            End If

                            If nPos3 > 0 Then
                                nPos2 = nPos3
                            End If
                        End If
                    End If
                Else
                    bRemove = False
                End If

                If bRemove Then
                    strResult = strResult & Left(strText, nPos1 - 1)
                    strText = Mid(strText, nPos2 + 1)
                Else
                    strResult = strResult & Left(strText, nPos1)
                    strText = Mid(strText, nPos1 + 1)
                End If
            Else
                strResult = strResult & strText
                strText = ""
            End If

            nPos1 = InStr(strText, "<")
        Loop
        strResult = strResult & strText

        RemoveHTML = strResult
    End Function

    Private Function SaveState(ByVal iTicket As Integer, ByVal strpriority As String, _
       ByVal struserssitename As String, _
       ByVal strusersemail As String, _
       ByVal strreqemail As String, _
       ByVal strreqbu As String, _
       ByVal strusersbu As String, _
       ByVal strreqsitenam As String, _
       ByVal strIncText As String, _
       ByVal strRequestor As String, _
       ByVal strUser As String, _
       ByVal strreqphone As String, _
       ByVal struserphone As String, _
       ByVal strmastersitename As String, _
       ByVal strmasteremail As String, _
       ByVal strmasterbu As String, _
       ByVal strMasterUser1 As String, _
       ByVal strMasterphone As String, _
       ByVal strMasterUser As String, ByVal connector As Object) As Integer

        'Dim bSaved As Boolean = False
        Dim strSQLstring As String = ""
        'Dim connector As OleDbConnection
        Dim rowsaffected As Integer = 0
        Dim iLine As Integer = 0
        Dim sParameters As String = ""
        sParameters = AddParameter("iTicket", iTicket.ToString) & " " & AddParameter("strpriority", strpriority) & " " & _
            AddParameter("strusersitename", struserssitename) & " " & AddParameter("strusersemail", strusersemail) & " " & _
            AddParameter("strreqemail", strreqemail) & " " & AddParameter("strreqbu", strreqbu) & " " & _
            AddParameter("strusersbu", strusersbu) & " " & AddParameter("strreqsitenam", strreqsitenam) & " " & _
            AddParameter("strRequestor", strRequestor) & " " & _
            AddParameter("strUser", strUser) & " " & AddParameter("strreqphone", strreqphone) & " " & _
            AddParameter("struserphone", struserphone) & " " & AddParameter("strmastersitename", strmastersitename) & " " & _
            AddParameter("strmasteremail", strmasteremail) & " " & AddParameter("strmasterbu", strmasterbu) & " " & _
            AddParameter("strMasterUser1", strMasterUser1) & " " & AddParameter("strMasterphone", strMasterphone) & " " & _
            AddParameter("strMasterUser", strMasterUser)

        Try

            strIncText = Regex.Replace(strIncText, "'", " ")
            iLine = 1
            strreqsitenam = Regex.Replace(strreqsitenam, "'", " ")
            iLine = 2
            strmastersitename = Regex.Replace(strmastersitename, "'", " ")
            iLine = 3
            struserssitename = Regex.Replace(struserssitename, "'", " ")
            iLine = 4

            strSQLstring = _
                      "INSERT INTO [Magic_Ticket_Master]" & vbCrLf & _
                            "([Magic_ticket_ID]" & vbCrLf & _
                            " ,[Magic_DatetimeAdded]" & vbCrLf & _
                            ",[Magic_Ticket_type]" & vbCrLf & _
                            ",[Magic_User_Department]" & vbCrLf & _
                            ",[Magic_Request_Type]" & vbCrLf & _
                            ",[Magic_Status]" & vbCrLf & _
                            ",[Magic_Priority]" & vbCrLf & _
                            ",[Magic_Open_Date]" & vbCrLf & _
                            ",[Magic_Due_Date]" & vbCrLf & _
                            ",[Magic_User]" & vbCrLf & _
                            ",[Magic_User_BU]" & vbCrLf & _
                            ",[Magic_User_Email]" & vbCrLf & _
                            ",[Magic_User_phone] " & vbCrLf & _
                            ",[Magic_User_Site_Name]" & vbCrLf & _
                            ",[Magic_Requestor]" & vbCrLf & _
                            ",[Magic_Request_BU]" & vbCrLf & _
                            ",[Magic_Request_Email]" & vbCrLf & _
                            ",[Magic_Req_phone]" & vbCrLf & _
                            ",[Magic_Request_Site_name]" & vbCrLf & _
                            ",[Magic_Master_User]" & vbCrLf & _
                            ",[Magic_Master_BU]" & vbCrLf & _
                            ",[Magic_Master_Site_Email]" & vbCrLf & _
                            ",[Magic_Master_Phone]" & vbCrLf & _
                            ",[Magic_Master_Site_Name]" & vbCrLf & _
                            ",[Magic_Estimated_time_comp]" & vbCrLf & _
                            ",[Magic_Specs_design_needed]" & vbCrLf & _
                            ",[Magic_Design_Comp]" & vbCrLf & _
                            ",[Magic_user_testing_needed]" & vbCrLf & _
                            ",[Magic_user_testing_completed]" & vbCrLf & _
                            ",[Magic_Ticket_Description]" & vbCrLf & _
                            ",[Magic_User_phone_large])" & vbCrLf & _
                                             " VALUES  (" & vbCrLf & _
                                " '" & iTicket.ToString & "'," & vbCrLf & _
                                 " getdate()," & vbCrLf & _
                                " 'Ticket'," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " 'preOpen'," & vbCrLf & _
                                " '" & strpriority & "'," & vbCrLf & _
                                " getdate()," & vbCrLf & _
                                " getdate()+ 7," & vbCrLf & _
                                " '" & strUser & "'," & vbCrLf & _
                                " '" & strusersbu & "'," & vbCrLf & _
                                " '" & strusersemail & "'," & vbCrLf & _
                                " '" & struserphone & "'," & vbCrLf & _
                                " '" & struserssitename & "'," & vbCrLf & _
                                " '" & strRequestor & "'," & vbCrLf & _
                                " '" & strreqbu & "'," & vbCrLf & _
                                " '" & strreqemail & "'," & vbCrLf & _
                                " '" & strreqphone & "'," & vbCrLf & _
                                " '" & strreqsitenam & "'," & vbCrLf & _
                                " '" & strMasterUser1 & "'," & vbCrLf & _
                                " '" & strmasterbu & " '," & vbCrLf & _
                                " '" & strmasteremail & "'," & vbCrLf & _
                                " '" & strMasterphone & "'," & vbCrLf & _
                                " '" & strmastersitename & " ' ," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " '" & strIncText & "', " & vbCrLf & _
                                "'" & struserphone & "')"
            iLine = 5

            rowsaffected = SQLDBAccess.ExecNonQuerySql(strSQLstring, connectSQL)
            iLine = 6
            clsLogger.Log_Event("EmailOpsStaff:GenericClass:SaveState : Insert Master Ticket record; iLine=" & iLine.ToString & _
                                " rowsaffected=" & rowsaffected.ToString)
            If Not rowsaffected = 1 Then
                clsLogger.Log_Event("EmailOpsStaff:GenericClass:SaveState ERROR : iLine=" & iLine.ToString & " rowsaffected=" & rowsaffected.ToString & _
                                    " sParameters=" & sParameters & " strSQLstring=" & strSQLstring)
                Return rowsaffected
            End If





            iLine = 7

            strSQLstring = _
                      "INSERT INTO [Magic_Ticket_Child]" & vbCrLf & _
                            "([Magic_ticket_ID]" & vbCrLf & _
                            " ,[M_DatetimeAdded]" & vbCrLf & _
                            ",[M_Status]" & vbCrLf & _
                            ",[M_Start_Date]" & vbCrLf & _
                            ",[M_Date_Assigned]" & vbCrLf & _
                            ",[M_Date_Completed]" & vbCrLf & _
                            ",[M_UserID]" & vbCrLf & _
                            ",[M_Resolution]" & vbCrLf & _
                            ",[M_Description]" & vbCrLf & _
                            ",[M_Assignto]" & vbCrLf & _
                            ",[M_Assignto_Dept]" & vbCrLf & _
                            ",[M_Assignto_phone] " & vbCrLf & _
                            ",[M_Assignto_Email]" & vbCrLf & _
                            ",[M_Assignto_Site_name]" & vbCrLf & _
                            ",[M_Requistioner])" & vbCrLf & _
                                " VALUES  (" & vbCrLf & _
                                " '" & iTicket.ToString & "'," & vbCrLf & _
                                " getdate()," & vbCrLf & _
                                " 'preOpen'," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " '" & strMasterUser & "'," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " '" & strIncText & "'," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                " ' '," & vbCrLf & _
                                "'" & strRequestor & "')"
            iLine = 8
        Catch ex As Exception
            clsLogger.Log_Event("EmailOpsStaff:GenericClass:SaveState ERROR: iLine=" & iLine.ToString & " strSQLstring=" & strSQLstring & _
                                " sParameters=" & sParameters & " ex.Message=" & ex.Message)
            Return rowsaffected
        End Try

        '
        'Dim command1 As OleDbCommand
        'command1 = New OleDbCommand(strSQLstring, connector)

        Try
            iLine = 9
            rowsaffected = SQLDBAccess.ExecNonQuerySql(strSQLstring, connectSQL)
            iLine = 10
            clsLogger.Log_Event("EmailOpsStaff:GenericClass:SaveState : Insert Child Ticket Record; iLine=" & iLine.ToString & _
                                " rowsaffected=" & rowsaffected.ToString)
            If Not rowsaffected = 1 Then
                clsLogger.Log_Event("EmailOpsStaff:GenericClass:SaveState ERROR: iLine=" & iLine.ToString & " rowsaffected=" & rowsaffected.ToString & _
                                    " sParameters=" & sParameters & " strSQLstring=" & strSQLstring)
                Return rowsaffected
            End If

        Catch ex As Exception
            clsLogger.Log_Event("EmailOpsStaff:GenericClass:SaveState ERROR: iLine=" & iLine.ToString & " rowsaffected=" & rowsaffected.ToString & _
                                " sParameters=" & sParameters & " strSQLstring=" & strSQLstring & " ex.Message=" & ex.Message)
            Return rowsaffected
        End Try

        Return rowsaffected
    End Function 'savestate()
    Function clsNewTicNO(ByVal intTickID As Integer, ByVal connector1 As SqlConnection) As Integer
        Dim iLine As Integer = 0
        Dim strSQLstring As String = ""
        Try

            intTickID = 0
            strSQLstring = "SELECT max(Magic_Ticket_number) " & vbCrLf & _
                      " FROM Magic_Numbers"
            iLine = 1
            intTickID = SQLDBAccess.GetSQLScalar(strSQLstring, connector1)
            iLine = 2
            intTickID = intTickID + 2
            iLine = 3
            strSQLstring = "update Magic_numbers set magic_ticket_number = '" & intTickID & "'"
            iLine = 4
            Dim rows_affected As Integer = SQLDBAccess.GetSQLScalar(strSQLstring, connector1)
            iLine = 5
            'If inttickID = 0 Then
            'Return rows_affected
            'FRED.Response.Write("<Script language='javascript'> alert('Cannot create ticket please call HelpDesk'); </script>")
            'FRED.Server.Transfer("MagicIndex.aspx", True)
            'ERROR OUT AND MOVE THE EMAIL TO THE UNPROCESSD FOLDE
            'SEND AN EMAIL TO PAM
            'End If
        Catch ex As Exception
            clsLogger.Log_Event("EmailOpsStaff:GenericClass:clsNewTicNO ERROR: iLine=" & iLine.ToString & " intTickID=" & intTickID.ToString & " strSQLstring=" & strSQLstring & " ex.Message=" & ex.Message)
        End Try
        Return intTickID
    End Function

    Private Sub SendEmailOld(currentItem As Item)

        Dim email As New MailMessage

        'The email address of the sender
        email.From = "TechSupport@sdi.com"

        'The email address of the recipient. 
        email.To = "pam.wolfe@sdi.com; pete.doyle@sdi.com"

        'email.Cc = "pam.wolfe@sdi.com"

        'The subject of the email
        email.Subject = "Error Creating a Ticket from Field OPS"

        'The Priority attached and displayed for the email
        email.Priority = System.Net.Mail.MailPriority.High

        email.BodyFormat = MailFormat.Html

        email.Body = "<html><body><table><tr><td>Emails from Field Ops has completed with errors, review incomplete folders.</td></tr>"

        'email.Body = email.Body & "<tr><td></td><a href='\\BDougherty_XP-l\logs'>\\BDougherty_XP-l\logs\</a></tr></table></body></html>"

        'Send the email and handle any error that occurs
        Try
            UpdEmailOut.UpdEmailOut.UpdEmailOut(email.Subject, email.From, email.To, "", "", "Y", email.Body, connectOR)
        Catch ex As Exception
            objStreamWriter.WriteLine("     Error - the email was not sent")
        End Try

    End Sub

    Private Sub SendEmail(currentItem As Item)

        Dim email As New System.Net.Mail.MailMessage
        Dim sRecipients() As String
        Dim i As Integer
        Dim sEmailAttachmentName As String

        'The email address of the sender
        email.From = New System.Net.Mail.MailAddress("TechSupport@sdi.com", "TechSupport@sdi.com")

        'The email address of the recipient. 
        sRecipients = GetRecipients(currentItem)  ' "pam.wolfe@sdi.com; pete.doyle@sdi.com"
        For i = 0 To sRecipients.GetUpperBound(0)
            email.To.Add(sRecipients(i))
        Next

        'email.Cc = "pam.wolfe@sdi.com"

        'The subject of the email
        email.Subject = "Ticket Request Returned" '"Error Creating a Ticket from Field OPS"

        'The Priority attached and displayed for the email
        email.Priority = System.Net.Mail.MailPriority.High

        email.Body = "Your ticket request is being returned because the format is incomplete. Please review the attached instructions, fix the error, and resubmit."

        sEmailAttachmentName = "Creating Help Desk Tickets from an Email.docx"
        If File.Exists(sEmailAttachmentName) Then
            Dim itemAttachment As New System.Net.Mail.Attachment(sEmailAttachmentName)
            email.Attachments.Add(itemAttachment)
        End If

        'Send the email and handle any error that occurs
        Try
            'UpdEmailOut.UpdEmailOut.UpdEmailOut(email.Subject, email.From, email.To, "", "", "Y", email.Body, connectOR)
            Dim myClient As New System.Net.Mail.SmtpClient

#If DEBUG Then
            myClient.Host = "localhost"
            myClient.Port = 25
            myClient.UseDefaultCredentials = True
            myClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.SpecifiedPickupDirectory
            myClient.PickupDirectoryLocation = "c:\smtp"
#Else
            'production only
            myClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.PickupDirectoryFromIis

#End If

            myClient.Send(email)

        Catch ex As Exception
            objStreamWriter.WriteLine("     Error - the email was not sent")
        End Try

    End Sub

    Private Sub sendemail(ByVal mailer As MailMessage)

        Try


            SmtpMail.Send(mailer)
        Catch ex As Exception
            objStreamWriter.WriteLine("     Error - in the sendemail to customer SUB")
        End Try
    End Sub

    Private Function updateSendEmailTbl(ByVal strBU As String, ByVal strOrderNo As String, ByVal strOrderStatus As String, ByVal strorigin As String) As Boolean

        Dim strSQLstring As String
        Dim rowsaffected As Integer
        strSQLstring = "UPDATE PS_ISA_ORDSTAT_EML" & vbCrLf & _
                       " SET EMAIL_DATETIME = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf & _
                       " WHERE BUSINESS_UNIT_OM = '" & strBU & "'" & vbCrLf & _
                       " AND ORDER_NO = '" & strOrderNo & "'" & vbCrLf & _
                       " AND ISA_ORDER_STATUS = '" & strOrderStatus & "'"

        Dim Command1 As OleDbCommand
        Command1 = New OleDbCommand(strSQLstring, connectOR)
        Try
            rowsaffected = Command1.ExecuteNonQuery
            If rowsaffected = 0 Then
                objStreamWriter.WriteLine("**")
                objStreamWriter.WriteLine("     Error - 0 PS_ISA_ORDSTAT_EML tbl for order " & strOrderNo)
                objStreamWriter.WriteLine("**")
                updateSendEmailTbl = True
            End If
        Catch OleDBExp As OleDbException
            objStreamWriter.WriteLine("**")
            objStreamWriter.WriteLine("     Error - updating PS_ISA_ORDSTAT_EML tbl for order " & strOrderNo)
            objStreamWriter.WriteLine("**")
            updateSendEmailTbl = True
        End Try
        Command1.Dispose()
    End Function

    Private Function FindSDIEmail(ByVal currentItem As Item, ByRef sItemEmailAddress As String, ByRef bSkipEmail As Boolean) As String
        Dim sSDIEmail As String = ""
        Dim iLine As Integer = 0
        Try

            sItemEmailAddress = " "
            'Dim itemPropertySet As PropertySet

            'itemPropertySet = New PropertySet(BasePropertySet.FirstClassProperties)
            'itemPropertySet.RequestedBodyType = BodyType.Text
            'currentItem.Load(itemPropertySet)

            Dim myEmailMessage As Microsoft.Exchange.WebServices.Data.EmailMessage
            iLine = 1
            myEmailMessage = CType(currentItem, Microsoft.Exchange.WebServices.Data.EmailMessage)
            iLine = 2
            myEmailMessage.Load()
            iLine = 3
            If myEmailMessage.Subject.ToUpper.StartsWith("RE:") Or _
                myEmailMessage.Subject.ToUpper.StartsWith("FW:") Or _
                myEmailMessage.Subject.ToUpper.StartsWith("FWD:") Then
                iLine = 31
                bSkipEmail = True
                clsLogger.Log_Event("EmailOpsStaff:GenericClass:FindSDIEmail set bSkipEmail to true; myEmailMessage.Subject=" & myEmailMessage.Subject)
            Else
                iLine = 32
                If IsSDIEmail(myEmailMessage.Sender.Address) Then
                    iLine = 4
                    sSDIEmail = myEmailMessage.Sender.Name
                    iLine = 5
                    If myEmailMessage.Sender.Address.IndexOf("@") >= 0 Then
                        iLine = 6
                        sItemEmailAddress = myEmailMessage.Sender.Address
                        iLine = 7
                    End If
                ElseIf Not myEmailMessage.DisplayCc Is Nothing Then
                    iLine = 8
                    Dim sCCEmails As String()
                    Dim i As Integer
                    sCCEmails = myEmailMessage.DisplayCc.Split(New Char() {";"c})
                    iLine = 9
                    i = 0
                    While (i < sCCEmails.Length And sSDIEmail = "")
                        iLine = 10
                        If IsEmailAddress(sCCEmails(i)) Then
                            iLine = 11
                            If IsSDIEmail(sCCEmails(i)) Then
                                iLine = 12
                                sSDIEmail = sCCEmails(i)
                                iLine = 13
                                If sCCEmails(i).IndexOf("@") >= 0 Then
                                    iLine = 14
                                    sItemEmailAddress = sCCEmails(i)
                                    iLine = 15
                                End If
                            End If
                        Else
                            iLine = 16
                            If IsSDIEmailFromDisplayName(sCCEmails(i)) Then
                                iLine = 17
                                sSDIEmail = sCCEmails(i)
                                iLine = 18
                                If sCCEmails(i).IndexOf("@") >= 0 Then
                                    iLine = 19
                                    sItemEmailAddress = sCCEmails(i)
                                End If
                            End If
                        End If

                        i = i + 1
                    End While
                End If
            End If
        Catch ex As Exception
            clsLogger.Log_Event("EmailOpsStaff:GenericClass:FindSDIEmail ERROR: iLine=" & iLine.ToString & " ex.Message=" & ex.Message)
        End Try

        Return sSDIEmail
    End Function

    Private Function IsEmailAddress(sEmail As String) As Boolean
        Dim bIsEmailAddress As Boolean = False

        If Not sEmail Is Nothing Then
            If sEmail.IndexOf("@") >= 0 Then
                bIsEmailAddress = True
            End If
        End If

        Return bIsEmailAddress
    End Function

    Private Function IsSDIEmail(sEmail As String) As Boolean
        Dim bRet As Boolean = False

        Try
            If Not sEmail Is Nothing Then
                If sEmail.ToUpper().IndexOf("@SDI.COM") >= 0 Then
                    bRet = True
                End If
            End If
        Catch ex As Exception
            clsLogger.Log_Event("EmailOpsStaff:GenericClass:IsSDIEmail ERROR: " & ex.Message)
        End Try
        Return bRet
    End Function

    Private Function IsSDIEmailFromDisplayName(sDisplayName As String) As Boolean
        Dim objUserTbl As clsUserTbl
        Dim bFound As Boolean = False
        Dim iLine As Integer = 0

        Try
            connectOR.Open()
            iLine = 1
            If sDisplayName.IndexOf(",") > 0 Then ' name comes in as "Doe, John"
                iLine = 2
                sDisplayName = Regex.Replace(sDisplayName, " ", "")
                iLine = 3
            Else
                iLine = 4
                Dim iSpaceLocation As Integer
                iSpaceLocation = sDisplayName.IndexOf(" ")
                iLine = 5
                If iSpaceLocation > 0 Then ' name comes in as "John Doe"
                    iLine = 6
                    sDisplayName = sDisplayName.Substring(iSpaceLocation + 1) + "," + sDisplayName.Substring(0, iSpaceLocation)
                    iLine = 7
                End If
            End If
            iLine = 8
            Dim bSuccess As Boolean = True
            objUserTbl = New clsUserTbl(sDisplayName, " ", connectOR, clsLogger, bSuccess)
            iLine = 9
            connectOR.Close()
            iLine = 10
            If IsSDIEmail(objUserTbl.EmployeeEmail) Then
                iLine = 11
                bFound = True
            End If
        Catch ex As Exception
            clsLogger.Log_Event("EmailOpsStaff:GenericClass:IsSDIEmailFromDisplayName iLine=" & iLine.ToString & " ERROR: " & ex.Message)
        End Try

        Return bFound
    End Function

    Private Function GetRecipients(currentItem As Item) As String()
        Dim sRecipients() As String
        Dim myEmailMessage As Microsoft.Exchange.WebServices.Data.EmailMessage
        Dim objUserTbl As clsUserTbl
        Dim bAddEmail As Boolean
        Dim i As Integer

        myEmailMessage = CType(currentItem, Microsoft.Exchange.WebServices.Data.EmailMessage)
        myEmailMessage.Load()

        ReDim sRecipients(0)
        sRecipients(0) = myEmailMessage.Sender.Address

        If Not myEmailMessage.DisplayCc Is Nothing Then
            Dim sCCEmails As String()
            sCCEmails = myEmailMessage.DisplayCc.Split(New Char() {";"c})
            For i = 0 To sCCEmails.GetUpperBound(0)
                bAddEmail = False
                If IsEmailAddress(sCCEmails(i)) Then
                    bAddEmail = True
                Else
                    connectOR.Open()
                    sCCEmails(i) = Regex.Replace(sCCEmails(i), " ", "")
                    Dim bSuccess As Boolean = True
                    objUserTbl = New clsUserTbl(sCCEmails(i), " ", connectOR, clsLogger, bSuccess)
                    connectOR.Close()
                    If IsEmailAddress(objUserTbl.EmployeeEmail) Then
                        sCCEmails(i) = objUserTbl.EmployeeEmail
                        bAddEmail = True
                    End If
                End If
                If bAddEmail Then
                    ReDim Preserve sRecipients(sRecipients.Length)
                    sRecipients(sRecipients.Length - 1) = sCCEmails(i)
                End If
            Next
        End If

        If ResponseEmailRecipients.Length > 0 Then
            Dim sAdditionalRecipients As String()
            sAdditionalRecipients = ResponseEmailRecipients.Split(New Char() {";"c})
            For i = 0 To sAdditionalRecipients.GetUpperBound(0)
                ReDim Preserve sRecipients(sRecipients.Length)
                sRecipients(sRecipients.Length - 1) = sAdditionalRecipients(i)
            Next
        End If

        Return sRecipients
    End Function

    Private Function AddParameter(sParamName As String, sParamValue As String)
        Dim sReturn As String = sParamName & "="

        If sParamValue Is Nothing Then
            sReturn &= " is Nothing"
        Else
            sReturn &= sParamValue
        End If
        Return sReturn
    End Function
	
	public sub SetORACN(byval oraCNString as string)
		connectOR = New OleDbConnection(oraCNString)
	end sub
	
	public sub SetSQLCN(byval sqlCNString as string)
		connectSQL = New SqlClient.SqlConnection(sqlCNString)
	end sub
	
End Class

