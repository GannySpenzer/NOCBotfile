Public Class TicketMgr

    Private m_sLastError As String
    Public Property LastError As String
        Get
            Return m_sLastError
        End Get
        Private Set(value As String)
            m_sLastError = value
        End Set
    End Property

    Private m_sTicketID As String
    Public Property TicketID As String
        Get
            Return m_sTicketID
        End Get
        Private Set(value As String)
            m_sTicketID = value
        End Set
    End Property

    Private m_bProductionEnvironment As Boolean
    Private Property ProductionEnvironment As Boolean
        Get
            Return m_bProductionEnvironment
        End Get
        Set(value As Boolean)
            m_bProductionEnvironment = value
        End Set
    End Property

    Public Sub New(bProductionEnvironment As Boolean)
        LastError = ""
        TicketID = ""
        ProductionEnvironment = bProductionEnvironment
    End Sub

    Public Function AddTicket(oTicketInfo As TicketInfo) As Boolean
        Dim bSuccess As Boolean = False

        ClearProperties()

        Try
            If GetNewTicketID() Then
                If AddMasterRecord(oTicketInfo) Then
                    If AddChildRecord(oTicketInfo) Then
                        bSuccess = True
                    End If
                End If
            End If

        Catch ex As Exception
            LastError = "Ticket.AddTicket: TicketID=" & TicketID & " ex.Message=" & ex.Message & " ex.StackTrace=" & ex.StackTrace
        End Try

        Return bSuccess
    End Function

    Private Sub ClearProperties()
        LastError = ""
        TicketID = ""
    End Sub

    Private Function GetNewTicketID() As Boolean
        Const cFunction As String = "GetNewTicketID"

        Dim bSuccess As Boolean = False
        Dim strSQLstring As String = ""

        Try
            ' read last-used ticket ID
            strSQLstring = "SELECT max(Magic_Ticket_number) " & vbCrLf & _
                       " FROM  [Magic_Numbers]"
            Dim sTickID As String = SQLDBData.GetProdSQLScalarSPC3(strSQLstring)
            Dim iTickID As Integer = CType(sTickID, Integer)

            ' increment last-used ticket ID and store it into the ticket number table
            iTickID = iTickID + 1
            strSQLstring = "update  [Magic_numbers] set magic_ticket_number = '" & iTickID.ToString & "'"
            Dim rows_affected As Integer = SQLDBData.ExecNonQuerySQLSPC3(strSQLstring)
            If rows_affected = 1 Then
                bSuccess = True
                TicketID = iTickID
            Else
                bSuccess = False
                LastError = "Ticket." & cFunction & ": rows_affected=" & rows_affected.ToString & " strSQLstring=" & strSQLstring
            End If

        Catch ex As Exception
            LastError = "Ticket." & cFunction & ": Unexpected error - strSQLstring=" & strSQLstring & " ex.Message=" & ex.Message & " ex.StackTrace=" & ex.StackTrace
            bSuccess = False
        End Try

        Return bSuccess
    End Function

    Private Function AddMasterRecord(oTicketInfo As TicketInfo) As Boolean
        Const cFunction As String = "AddMasterRecord"
        Const cStatus As String = "Assigned"
        Const cUserDept As String = " "
        Const cRequestType As String = " "
        Const cDueDate As String = " "
        Const cEstTimeComp As String = " "
        Const cSpecsDesignNeeded As String = " "
        Const cDesignComp As String = " "
        Const cUserTestingNeeded As String = " "
        Const cUserTestingCompleted As String = " "

        Dim bSuccess As Boolean = False
        Dim strSQLstring As String = " "

        Try
            If oTicketInfo.RequestorInfo.ExistsUser Then
                If oTicketInfo.AssigneeInfo.ExistsUser Then
                    strSQLstring = _
                        "INSERT INTO  [Magic_Ticket_Master]" & vbCrLf & _
                                "([Magic_ticket_ID], [Magic_DatetimeAdded], [Magic_Ticket_type]" & vbCrLf & _
                                ",[Magic_User_Department], [Magic_Request_Type], [Magic_Status]" & vbCrLf & _
                                ",[Magic_Priority], [Magic_Open_Date], [Magic_Due_Date]" & vbCrLf & _
                                ",[Magic_User], [Magic_User_BU], [Magic_User_Email]" & vbCrLf & _
                                ",[Magic_User_phone], [Magic_User_Site_Name], [Magic_Requestor]" & vbCrLf & _
                                ",[Magic_Request_BU], [Magic_Request_Email], [Magic_Req_phone]" & vbCrLf & _
                                ",[Magic_Request_Site_name], [Magic_Master_User], [Magic_Master_BU]" & vbCrLf & _
                                ",[Magic_Master_Site_Email], [Magic_Master_Phone], [Magic_Master_Site_Name]" & vbCrLf & _
                                ",[Magic_Estimated_time_comp], [Magic_Specs_design_needed], [Magic_Design_Comp]" & vbCrLf & _
                                ",[Magic_user_testing_needed], [Magic_user_testing_completed], [Magic_Ticket_Description]" & vbCrLf & _
                                ",[Magic_User_phone_large])" & vbCrLf & _
                        " VALUES  (" & vbCrLf & _
                                    " '" & TicketID & "', getdate(), '" & oTicketInfo.TicketType & "'," & vbCrLf & _
                                    " '" & cUserDept & "', '" & cRequestType & "', '" & cStatus & "'," & vbCrLf & _
                                    " '" & oTicketInfo.Priority & "', getdate(), '" & cDueDate & "'," & vbCrLf & _
                                    " '" & oTicketInfo.RequestorID & "', '" & oTicketInfo.RequestorInfo.BU & "', '" & oTicketInfo.RequestorInfo.Email & "'," & vbCrLf & _
                                    " '" & oTicketInfo.RequestorInfo.Phone & "', '" & oTicketInfo.RequestorInfo.SiteName & "', '" & oTicketInfo.RequestorID & "'," & vbCrLf & _
                                    " '" & oTicketInfo.RequestorInfo.BU & "', '" & oTicketInfo.RequestorInfo.Email & "', '" & oTicketInfo.RequestorInfo.Phone & "'," & vbCrLf & _
                                    " '" & oTicketInfo.RequestorInfo.SiteName & "', '" & oTicketInfo.AssigneeID.ToUpper & "', '" & oTicketInfo.AssigneeInfo.BU & " '," & vbCrLf & _
                                    " '" & oTicketInfo.AssigneeInfo.EmailUser & "', '" & oTicketInfo.AssigneeInfo.PhoneUser & "', '" & oTicketInfo.AssigneeInfo.SiteName & " ' ," & vbCrLf & _
                                    " '" & cEstTimeComp & "', '" & cSpecsDesignNeeded & "', '" & cDesignComp & "'," & vbCrLf & _
                                    " '" & cUserTestingNeeded & "', '" & cUserTestingCompleted & "', '" & oTicketInfo.Description.Replace("'", "''") & "', " & vbCrLf & _
                                    "'" & oTicketInfo.RequestorInfo.Phone & "')"

                    Dim rows_affected As Integer
                    rows_affected = SQLDBData.ExecNonQuerySQLSPC3(strSQLstring)

                    If rows_affected <> 1 Then
                        LastError = "Ticket." & cFunction & " ERROR: rows_affected = " & rows_affected.ToString & " while adding master record; strSQLstring=" & strSQLstring
                        bSuccess = False
                    Else
                        bSuccess = True
                    End If
                Else
                    bSuccess = False
                    LastError = "Ticket." & cFunction & " ERROR: Assignee " & oTicketInfo.AssigneeID & " does not exist in users table ps_isa_users_tbl; strSQLstring=" & strSQLstring
                End If
            Else
                bSuccess = False
                LastError = "Ticket." & cFunction & " ERROR: Requestor " & oTicketInfo.RequestorID & " does not exist in users table ps_isa_users_tbl; strSQLstring=" & strSQLstring
            End If

        Catch ex As Exception
            LastError = "Ticket." & cFunction & " - unexpected error - strSQLstring=" & strSQLstring & " ex.Message=" & ex.Message & " ex.StackTrace=" & ex.StackTrace
        End Try

        Return bSuccess
    End Function

    Private Function AddChildRecord(oTicketInfo As TicketInfo) As Boolean
        Const cFunction As String = "AddChildRecord"
        Const cStatusAssigned As String = "Assigned"
        Const cStartDate As String = " "
        Const cDateCompleted As String = " "
        Const cResolution As String = " "

        Dim bSuccess As Boolean = False
        Dim strSQLstring As String = " "

        Try
            strSQLstring = _
                        "INSERT INTO [Magic_Ticket_Child]" & vbCrLf & _
                            "([Magic_ticket_ID], [M_DatetimeAdded], [M_Status]" & vbCrLf & _
                            ",[M_Start_Date], [M_Date_Assigned], [M_Date_Completed]" & vbCrLf & _
                            ",[M_UserID], [M_Resolution], [M_Description]" & vbCrLf & _
                            ",[M_Assignto], [M_Assignto_Dept], [M_Assignto_phone] " & vbCrLf & _
                            ",[M_Assignto_Email], [M_Assignto_Site_name], [M_Requistioner])" & vbCrLf & _
                        " VALUES  (" & vbCrLf & _
                                " '" & TicketID & "', getdate(), '" & cStatusAssigned & "'," & vbCrLf & _
                                " '" & cStartDate & "', getdate(), '" & cDateCompleted & "'," & vbCrLf & _
                                " '" & oTicketInfo.RequestorID.ToUpper & "', '" & cResolution & "', '" & oTicketInfo.Description.Replace("'", "''") & "'," & vbCrLf & _
                                " '" & oTicketInfo.AssigneeID.ToUpper & "', '" & oTicketInfo.AssigneeInfo.Department & "', '" & oTicketInfo.AssigneeInfo.PhoneAssignee & "'," & vbCrLf & _
                                " '" & oTicketInfo.AssigneeInfo.EmailAssignee & "', '" & oTicketInfo.AssigneeInfo.Department & "', '" & oTicketInfo.RequestorID & "')"

            Dim rows_affected As Integer
            rows_affected = SQLDBData.ExecNonQuerySQLSPC3(strSQLstring)
            If rows_affected < 1 Then
                bSuccess = False
                LastError = "Ticket." & cFunction & " ERROR: rows_affected=" & rows_affected.ToString & " while adding child record; strSQLstring=" & strSQLstring
            Else
                bSuccess = True
            End If
        Catch ex As Exception
            bSuccess = False
            LastError = "Ticket." & cFunction & " - unexpected error - strSQLstring=" & strSQLstring & " ex.Message=" & ex.Message & " ex.StackTrace=" & ex.StackTrace
        End Try

        Return bSuccess
    End Function

    Private Function CreateEmailBodyHead() As String
        Dim strBodyhead As String = ""

        strBodyhead = strBodyhead & "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI Ticket - Assigned-To Notice</span><center>" & vbCrLf
        strBodyhead = strBodyhead & "&nbsp;" & vbCrLf

        Return strBodyhead
    End Function

    Private Function CreateEmailBodyDetail(oTicketInfo As TicketInfo) As String
        Dim strbodydetl As String = ""

        strbodydetl = "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<HR width='100%' SIZE='1'>" & vbCrLf
        strbodydetl = strbodydetl & "&nbsp;" & vbCrLf
        strbodydetl = strbodydetl & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span >You have a new Ticket assigned to you.  " & " </span>&nbsp;"
        strbodydetl = strbodydetl & oTicketInfo.AssigneeInfo.FirstName & " " & oTicketInfo.AssigneeInfo.LastName & "</td></TR>" & vbCrLf

        'skip a line after the summary
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD colspan='2'>" & vbCrLf
        strbodydetl = strbodydetl & "&nbsp;</td></tr>" & vbCrLf

        'ticket number
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<b>Ticket Number:</b><span> &nbsp;" & TicketID & " </span></td></tr>"

        'ticket description
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<b>Ticket Description:</b></td></tr>"

        For i As Integer = 0 To oTicketInfo.TaskArray.Count - 1
            strbodydetl = strbodydetl & "<TR>" & vbCrLf
            strbodydetl = strbodydetl & "<TD>" & vbCrLf
            'strbodydetl = strbodydetl & "<span>" & oTicketInfo.Description & "</span>"
            strbodydetl = strbodydetl & "<span>" & oTicketInfo.TaskArray(i).ToString & "</span>"
            strbodydetl = strbodydetl & "</td></tr>"
        Next

        'skip a line after each user's description
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD colspan='2'>" & vbCrLf
        strbodydetl = strbodydetl & "&nbsp;</td></tr>" & vbCrLf

        'ticket resolution
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<b>Ticket Resolution:</b></td></tr>"
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & " " & "</td></tr>"

        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf

        Dim sResolution As String = " "
        strbodydetl = strbodydetl & "<span>" & sResolution & "</span></td></tr>"

        'skip a line after each user's resolution
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD colspan='2'>" & vbCrLf
        strbodydetl = strbodydetl & "&nbsp;</td></tr>" & vbCrLf

        'SDI contact name
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<b>SDI Contact Name:</b><span>&nbsp;" & oTicketInfo.AssigneeInfo.AssigneeName & "</span></td></tr>"

        'contact phone
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<b>Contact Phone:</b><span>&nbsp;" & oTicketInfo.AssigneeInfo.PhoneAssignee & "</span></td></tr>" & vbCrLf

        'skip a few lines...
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf

        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf

        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf

        strbodydetl = strbodydetl & "<TR>" & vbCrLf
        strbodydetl = strbodydetl & "<TD>" & vbCrLf
        strbodydetl = strbodydetl & "&nbsp;<br>" & vbCrLf

        Return strbodydetl
    End Function

    Public Function SendEmail(oTicketInfo As TicketInfo) As Boolean
        Const cFunction As String = "SendEmail"

        Dim bSuccess As Boolean = False

        Try
            If TicketID.Trim.Length = 0 Then
                bSuccess = False
                LastError = "Ticket." & cFunction & " - Cannot send email; there is no ticket ID"
            Else
                Const cProcessType As String = "Mail"
                Const cFromAddress As String = "SDIExchange@SDI.com" '"SDIExchADMIN@sdi.com" ' "SDIExchange@SDI.com"
                Const cMailCategory As String = "AutoTickGen"

                Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
                Dim MailAttachmentName As String() = Nothing
                Dim MailAttachmentbytes As New List(Of Byte())()
                Dim sEmailReturn As String
                Dim sMailSubject As String = "AutoTickGen - Ticket " & TicketID & " - " & oTicketInfo.EmailSubject '"Help Desk - Ticket " & sTicketID & " Assigned - To Notice"
                If Not ProductionEnvironment Then
                    sMailSubject = "<<TEST>> " & sMailSubject
                End If
                Dim sMailCC As String = ""
                Dim sMailBcc As String = ""
                Dim strBodyhead As String = CreateEmailBodyHead()
                Dim strbodydetl As String = CreateEmailBodyDetail(oTicketInfo)
                Dim sMailBody As String = strBodyhead & strbodydetl

                For i As Integer = 0 To oTicketInfo.EmailCCList.Count - 1
                    Dim sCC As String = oTicketInfo.EmailCCList(i)

                    If sCC.IndexOf("@") < 0 Then
                        Dim oUserCC As New UserInfo(oTicketInfo.EmailCCList(i))
                        If oUserCC.ExistsUser Then
                            sCC = oUserCC.Email
                        Else
                            LastError = "Ticket." & cFunction & ": Email CC user " & oTicketInfo.EmailCCList(i) & " does not exist"
                        End If
                    End If
                    If sCC.Trim.Length > 0 Then
                        If sMailCC.Trim.Length > 0 Then
                            sMailCC &= ";"
                        End If
                        sMailCC &= sCC
                    End If
                Next

            Dim sEmailUser As String = oTicketInfo.AssigneeInfo.EmailUser
            sEmailReturn = SDIEmailService.EmailUtilityServices(cProcessType, _
                                                 cFromAddress, _
                                                 sEmailUser, _
                                                 sMailSubject, _
                                                 sMailCC, _
                                                 sMailBcc, _
                                                 sMailBody, _
                                                 cMailCategory, _
                                                 MailAttachmentName, _
                                                 MailAttachmentbytes.ToArray())
            If sEmailReturn.Trim.Length = 0 Then
                LastError = "Ticket." & cFunction & " - Email may not have been sent to " & oTicketInfo.AssigneeInfo.EmailUser
                If sMailCC.Trim.Length > 0 Then
                    LastError &= " and " & sMailCC
                End If
                LastError &= "; return from SDIEmailService.EmailUtilityServices is blank indicating a problem."
                bSuccess = False
            Else
                bSuccess = True
            End If
            End If

        Catch ex As Exception
            bSuccess = False
            LastError = "Ticket." & cFunction & " - unexpected error - ex.Message=" & ex.Message & " ex.StackTrace=" & ex.StackTrace
        End Try

        Return bSuccess
    End Function

End Class
