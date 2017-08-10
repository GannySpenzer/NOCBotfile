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
    Dim connectOR As New OleDbConnection("Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=STAR")
    Dim connectSQL As New SqlClient.SqlConnection("server=192.168.253.16;uid=sa;pwd=sdiadmin;initial catalog='pubs'")
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

    Overrides Function ProcessItem(ByVal currentItem As Item) As Boolean
        Dim bReturn As Boolean = True
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


        Try
            strcc = FindSDIEmail(currentItem)
            'strcc = currentItem.DisplayCc.ToString
            'Dim tt As String = Microsoft.Exchange.WebServices.Data.ExchangeService.
            If strcc = "" Then
                clsLogger.Log_Event("GenericClass:Processing Item ERROR: No SDI email address")
                SendEmail(currentItem)
                bReturn = False
                Return bReturn
            End If

        Catch ex As Exception
            clsLogger.Log_Event("GenericClass:Processing Item ERROR: " & ex.Message)
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

            'If SaveToFolderEnabled Then
            'zzzzzzzzz
            Dim itemPropertySet As PropertySet
            itemPropertySet = New PropertySet(BasePropertySet.FirstClassProperties)
            itemPropertySet.RequestedBodyType = BodyType.Text
            currentItem.Load(itemPropertySet)

            If currentItem.Body.Text Is Nothing Then
                bLogBodyError = True
            ElseIf currentItem.Body.Text.Trim().Length = 0 Then
                bLogBodyError = True
            End If
            If bLogBodyError Then
                clsLogger.Log_Event("GenericClass:Processing Item ERROR: Email body is empty")
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
            Dim shawn As EmailMessage
            Dim returnValue As EmailMessage
            Dim Service As New ExchangeService(ExchangeVersion.Exchange2007_SP1)
            Service.Credentials = New WebCredentials(UserName, Password)
            Service.Url = New Uri(ExchangeUrl, UriKind.Absolute)

            Dim propertySet1 As New PropertySet(BasePropertySet.FirstClassProperties, ItemSchema.Id)


            returnValue = EmailMessage.Bind(Service, strKey, propertySet1)

            'strip the html crap off of the message body in the email

            'For Each mybody As MessageBody In currentItem.Body.Text
            '    strMessagebody = strMessagebody + mybody.ToString
            'Next
            'strMessagebody = RemoveHTML(strMessagebody)
            'strMessagebody = Regex.Replace(strMessagebody, "<o:p>", " ")
            'strMessagebody = Regex.Replace(strMessagebody, "&nbsp;", " ")
            'strMessagebody = Regex.Replace(strMessagebody, "</o:p>", " ")
            strMessagebody = currentItem.Body.Text.Trim()

            Dim myemailSender As String = " "
            myemailSender = strcc
            myemailSender = Regex.Replace(myemailSender, " ", "")
            connectOR.Open()
            Dim objUserTbl As clsUserTbl
            objUserTbl = New clsUserTbl(myemailSender, " ", connectOR)



            Dim strCustName As String = objUserTbl.EmployeeName
            Dim strCustEmail As String = objUserTbl.EmployeeEmail
            Dim strCustPhone As String = objUserTbl.PhoneNum
            Dim strreqsitenam As String = objUserTbl.SiteName
            Dim struserssitename As String = objUserTbl.SiteName
            Dim strreqbu As String = objUserTbl.Business_unit
            Dim strusersbu As String = objUserTbl.Business_unit
            Dim strusersemail As String = objUserTbl.EmployeeEmail
            Dim strIncText As String = strMessagebody
            Dim strRequestor As String = objUserTbl.EmployeeID
            Dim struser As String = objUserTbl.EmployeeID
            Dim strmastersitename As String = objUserTbl.SiteName
            Dim strmasteremail As String = objUserTbl.EmployeeEmail
            Dim strmasterbu As String = objUserTbl.Business_unit
            Dim strMasterUser1 As String = objUserTbl.EmployeeID
            Dim strMasterphone As String = objUserTbl.PhoneNum
            Dim strMasterUser As String = objUserTbl.EmployeeID
            connectOR.Close()
            'Dim objTicket As New ClsInsertTicket(parm)
            ' get a unique ticket number
            Dim intTickid As Integer = clsNewTicNO(intTickid, connectSQL)
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
            Try
                If insertStatus <> 1 Then

                End If
            Catch ex As Exception
                clsLogger.Log_Event("GenericClass:Processing Item no ticket written ERROR: " & ex.Message)
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

    Private Function SaveState(ByVal strTicket As Integer, ByVal strpriority As String, _
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
       ByVal strMasterUser As String, ByVal connector As Object) As Boolean
        'Dim bSaved As Boolean = False
        Dim strSQLstring As String
        'Dim connector As OleDbConnection

        strIncText = Regex.Replace(strIncText, "'", " ")
        strreqsitenam = Regex.Replace(strreqsitenam, "'", " ")
        strmastersitename = Regex.Replace(strmastersitename, "'", " ")
        struserssitename = Regex.Replace(struserssitename, "'", " ")

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
                            " '" & strTicket & "'," & vbCrLf & _
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
        

        Dim rowsaffected As Integer
        rowsaffected = SQLDBAccess.ExecNonQuerySql(strSQLstring, connectSQL)
        If Not rowsaffected = 1 Then
            Return rowsaffected
        End If





        

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
                            " '" & strTicket & "'," & vbCrLf & _
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

        '
        'Dim command1 As OleDbCommand
        'command1 = New OleDbCommand(strSQLstring, connector)

        Try 
            rowsaffected = SQLDBAccess.ExecNonQuerySql(strSQLstring, connectSQL)
            If Not rowsaffected = 1 Then
                Return rowsaffected
            End If

        Catch ex As Exception
            Return rowsaffected
        End Try
         
        Return rowsaffected
    End Function 'savestate()
    Function clsNewTicNO(ByVal inttickID As Integer, ByVal connector1 As SqlConnection)
        inttickID = 0
        Dim strSQLstring As String = "SELECT max(Magic_Ticket_number) " & vbCrLf & _
                  " FROM Magic_Numbers"
        inttickID = SQLDBAccess.GetSQLScalar(strSQLstring, connector1)
        inttickID = inttickID + 2
        strSQLstring = "update Magic_numbers set magic_ticket_number = '" & inttickID & "'"
        Dim rows_affected As Integer = SQLDBAccess.GetSQLScalar(strSQLstring, connector1)
        Return inttickID
        If inttickID = 0 Then
            'Return rows_affected
            'FRED.Response.Write("<Script language='javascript'> alert('Cannot create ticket please call HelpDesk'); </script>")
            'FRED.Server.Transfer("MagicIndex.aspx", True)
            'ERROR OUT AND MOVE THE EMAIL TO THE UNPROCESSD FOLDE
            'SEND AN EMAIL TO PAM
        End If
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

    Private Function FindSDIEmail(ByVal currentItem As Item) As String
        Dim sSDIEmail As String = ""
        'Dim itemPropertySet As PropertySet

        'itemPropertySet = New PropertySet(BasePropertySet.FirstClassProperties)
        'itemPropertySet.RequestedBodyType = BodyType.Text
        'currentItem.Load(itemPropertySet)

        Dim myEmailMessage As Microsoft.Exchange.WebServices.Data.EmailMessage
        myEmailMessage = CType(currentItem, Microsoft.Exchange.WebServices.Data.EmailMessage)
        myEmailMessage.Load()
        If IsSDIEmail(myEmailMessage.Sender.Address) Then
            sSDIEmail = myEmailMessage.Sender.Name
        ElseIf Not myEmailMessage.DisplayCc Is Nothing Then
            Dim sCCEmails As String()
            Dim i As Integer
            sCCEmails = myEmailMessage.DisplayCc.Split(New Char() {";"c})
            i = 0
            While (i < sCCEmails.Length And sSDIEmail = "")
                If IsEmailAddress(sCCEmails(i)) Then
                    If IsSDIEmail(sCCEmails(i)) Then
                        sSDIEmail = sCCEmails(i)
                    End If
                Else
                    If IsSDIEmailFromDisplayName(sCCEmails(i)) Then
                        sSDIEmail = sCCEmails(i)
                    End If
                End If

                i = i + 1
            End While
        End If

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

        If Not sEmail Is Nothing Then
            If sEmail.ToUpper().IndexOf("@SDI.COM") >= 0 Then
                bRet = True
            End If
        End If
        Return bRet
    End Function

    Private Function IsSDIEmailFromDisplayName(sDisplayName As String) As Boolean
        Dim objUserTbl As clsUserTbl
        Dim bFound As Boolean = False

        Try
            connectOR.Open()
            If sDisplayName.IndexOf(",") > 0 Then ' name comes in as "Doe, John"
                sDisplayName = Regex.Replace(sDisplayName, " ", "")
            Else
                Dim iSpaceLocation As Integer
                iSpaceLocation = sDisplayName.IndexOf(" ")
                If iSpaceLocation > 0 Then ' name comes in as "John Doe"
                    sDisplayName = sDisplayName.Substring(iSpaceLocation + 1) + "," + sDisplayName.Substring(0, iSpaceLocation)
                End If
            End If
            objUserTbl = New clsUserTbl(sDisplayName, " ", connectOR)
            connectOR.Close()
            If IsSDIEmail(objUserTbl.EmployeeEmail) Then
                bFound = True
            End If
        Catch ex As Exception

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
                    objUserTbl = New clsUserTbl(sCCEmails(i), " ", connectOR)
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

End Class

