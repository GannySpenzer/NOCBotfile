Imports System.Data.OleDb
Imports System.Configuration

Module Module1

    Dim m_oLogger As Logger

    Sub Main()
        InitializeLogger()

        LogMessage("Main", "Started utility Populate_BU_UNSPSC_Tbl")
        LogMessage("Main", "PeopleSoft connection string : " & ORDBData.DbUrl)
        LogMessage("Main", "Unilog connection string : " & ORDBData.UnilogDbUrl)

        Dim dsUnilogBU_UNSPSC As New DataSet
        If GetUnilogUNSPSCCodes(dsUnilogBU_UNSPSC) Then
            PopulateTable(dsUnilogBU_UNSPSC)
        End If

        Dim strDbOracle As String = ORDBData.DbUrl

        LogMessage("Main", "Ended utility Populate_BU_UNSPSC_Tbl")
    End Sub

    Private Sub InitializeLogger()
        Dim sLogPath As String = String.Empty
        'If Not sLogPath.EndsWith("\") Then
        '    sLogPath &= "\"
        'End If
        'sLogPath &= "Logs"
        sLogPath = ConfigurationManager.AppSettings("LogPath")
        m_oLogger = New Logger(sLogPath, "Populate_BU_UNSPSC_Tbl")
    End Sub

    Private Function GetUnilogUNSPSCCodes(ByRef dsUnilogBU_UNSPSC As DataSet) As Boolean
        Dim bSuccess As Boolean = False
        Dim strSQLstring As String
        'strSQLstring = "SELECT DISTINCT P.subset_id, V.unspsc " & vbCrLf & _
        '    " FROM item_prices P, search_item_master_view_v2 V " & vbCrLf & _
        '    " WHERE P.item_id = V.item_id " & vbCrLf & _
        '" AND V.unspsc IS NOT NULL "
        strSQLstring = "SELECT DISTINCT P.subset_id, V.unspsc " & vbCrLf & _
            " FROM item_prices P, ITEM_MASTER V " & vbCrLf & _
            " WHERE P.item_id = V.item_id " & vbCrLf & _
        " AND V.unspsc IS NOT NULL "

        dsUnilogBU_UNSPSC = New DataSet

        Try
            dsUnilogBU_UNSPSC = ORDBData.UnilogGetAdapter(strSQLstring)
            Dim iCount As Integer = 0
            Try
                iCount = dsUnilogBU_UNSPSC.Tables(0).Rows.Count
            Catch ex As Exception
                iCount = 0
            End Try
            LogMessage("GetUnilogUNSPSCCodes", "Number of records read from Unilog tables item_prices and search_item_master_view_v2 : " & iCount.ToString)

            bSuccess = True
        Catch ex As Exception
            bSuccess = False
            LogMessage("GetUnilogUNSPSCCodes", "Error trying to get the Unilog subset IDs and UNSPSC codes.", ex)
            LogMessage("GetUnilogUNSPSCCodes", "ORDBData.UnilogDbUrl : " & ORDBData.UnilogDbUrl)
            LogMessage("GetUnilogUNSPSCCodes", "strSQLstring : " & strSQLstring)
        End Try

        Return bSuccess
    End Function

    Private Sub LogMessage(sFunctionName As String, sMessage As String, ex As Exception)
        Dim sLogMessage As String
        sLogMessage = sFunctionName & " : " & sMessage & " " & vbCrLf & _
            ex.Message & " " & vbCrLf
        If ex.InnerException IsNot Nothing Then
            sLogMessage = sLogMessage & ex.InnerException.Message & " " & vbCrLf
        End If
        sLogMessage = sLogMessage & ex.StackTrace

        m_oLogger.WriteLine(sLogMessage)

        SendEmailAlert(sLogMessage)
    End Sub

    Private Sub LogMessage(sFunctionName As String, sMessage As String)
        m_oLogger.WriteLine(sFunctionName & " : " & sMessage)
    End Sub

    Private Function PurgeBU_UNSPSC_Tbl(trnsactSession As OleDbTransaction, connection As OleDbConnection) As Boolean
        Dim bSuccess As Boolean = False
        Dim strSQLstring As String
        Dim iCount As Integer

        strSQLstring = "DELETE FROM sdix_bu_unspsc"

        Try
            iCount = ORDBData.ExecuteNonQuery(trnsactSession, connection, strSQLstring)
            LogMessage("PurgeBU_UNSPSC_Tbl", "Number of records purged from table SDIX_BU_UNSPSC: " & iCount.ToString)
            bSuccess = True
        Catch ex As Exception
            bSuccess = False
            LogMessage("PurgeBU_UNSPSC_Tbl", "Error trying to purge all records from table SDIX_BU_UNSPSC", ex)
            LogMessage("PurgeBU_UNSPSC_Tbl", "strSQLstring : " & strSQLstring)
        End Try

        Return bSuccess
    End Function

    Private Function PopulateBU_UNSPSC_Tbl(trnsactSession As OleDbTransaction, connection As OleDbConnection, dsUnilogBU_UNSPSC As DataSet) As Boolean
        Dim bSuccess As Boolean = False
        Dim strSQLstring As String = ""
        Dim iRowsAffected As Integer = 0
        Dim iInsertCount As Integer = 0
        Dim strError As String = ""
        Dim strFullErrorsList As String = ""

        Try
            For Each drSubsetID As DataRow In dsUnilogBU_UNSPSC.Tables(0).Rows
                'Dim sSubsetId As String = drSubsetID.Item("subset_id").ToString
                'Dim sZeroPadding As String = "00000".Substring(0, 4 - sSubsetId.Length)
                Dim sBU As String = drSubsetID.Item("subset_id").ToString  '   "I" & sZeroPadding & sSubsetId
                
                Dim sUNSPSC As String = drSubsetID.Item("unspsc").ToString

                strSQLstring = "INSERT INTO sdix_bu_unspsc " & vbCrLf & _
                    " ( isa_business_unit, unspsc ) " & vbCrLf & _
                    " SELECT " & vbCrLf & _
                    "       '" & sBU & "', " & vbCrLf & _
                    "       '" & sUNSPSC & "' " & vbCrLf & _
                    " FROM DUAL " & vbCrLf & _
                    " WHERE NOT EXISTS ( " & vbCrLf & _
                    "       SELECT 'x' FROM sdix_bu_unspsc " & vbCrLf & _
                    "       WHERE isa_business_unit = '" & sBU & "' " & vbCrLf & _
                    "       AND unspsc = '" & sUNSPSC & "' " & vbCrLf & _
                    " ) "

                strError = ""
                iRowsAffected = ORDBData.ExecuteNonQuery(trnsactSession, connection, strSQLstring, strError)
                If iRowsAffected > 0 Then
                    iInsertCount = iInsertCount + 1
                Else
                    If Trim(strError) <> "" Then
                        LogMessage("PopulateBU_UNSPSC_Tbl", "Error trying to populate table SDIX_BU_UNSPSC: " & strError)
                        LogMessage("PopulateBU_UNSPSC_Tbl", "strSQLstring : " & strSQLstring)
                        strFullErrorsList = strFullErrorsList & "<TR><TD><b>Error trying to populate table SDIX_BU_UNSPSC</b>: " & strError & "</td></tr>" & vbCrLf
                        strFullErrorsList = strFullErrorsList & "<TR><TD><b>strSQLstring</b>: " & strSQLstring & "</td></tr>" & vbCrLf

                        'strbodydetl = strbodydetl & "<TR>" & vbCrLf
                        'strbodydetl = strbodydetl & "<TD>" & vbCrLf
                        'strbodydetl = strbodydetl & "<b>Error(s) List: </b><span> &nbsp;" & strErrorMessage & "</span></td></tr>"
                    End If
                End If
            Next

            LogMessage("PopulateBU_UNSPSC_Tbl", "Number of records inserted into table SDIX_BU_UNSPSC: " & iInsertCount.ToString)

            bSuccess = True

            If Trim(strFullErrorsList) <> "" Then
                Try
                    SendEmail(strFullErrorsList)
                Catch exMail As Exception

                End Try
            End If
            
        Catch ex As Exception
            bSuccess = False
            LogMessage("PopulateBU_UNSPSC_Tbl", "Error trying to populate table SDIX_BU_UNSPSC.", ex)
            LogMessage("PopulateBU_UNSPSC_Tbl", "strSQLstring : " & strSQLstring)
        End Try

        Return bSuccess
    End Function

    Private Sub PopulateTable(dsUnilogBU_UNSPSC As DataSet)
        Dim trnsactSession As OleDbTransaction = Nothing
        Dim connection As OleDbConnection = New OleDbConnection(ORDBData.DbUrl)
        connection.Open()
        trnsactSession = connection.BeginTransaction

        Try
            If PurgeBU_UNSPSC_Tbl(trnsactSession, connection) Then
                If PopulateBU_UNSPSC_Tbl(trnsactSession, connection, dsUnilogBU_UNSPSC) Then
                    trnsactSession.Commit()
                    connection.Close()
                    trnsactSession = Nothing
                    connection = Nothing
                Else
                    RollBackTransaction(trnsactSession, connection)
                End If
            Else
                RollBackTransaction(trnsactSession, connection)
            End If

        Catch ex As Exception
            LogMessage("PopulateTable", "Error trying to populate table SDIX_BU_UNSPSC", ex)
            RollBackTransaction(trnsactSession, connection)
        End Try
    End Sub

    Private Sub RollBackTransaction(trnsactSession As OleDbTransaction, connection As OleDbConnection)
        Try
            If trnsactSession IsNot Nothing Then
                trnsactSession.Rollback()
                connection.Close()
            End If
        Catch ex1 As Exception
        End Try

        LogMessage("RollBackTransaction", "Rolled Back - due to errors")

        trnsactSession = Nothing
        connection = Nothing
    End Sub

    Private Sub SendEmailAlert(sLastErrorMessage As String)
        Try
            Const cErrMsg As String = "Utility Populate_BU_UNSPSC_Tbl had a critical error"
            Dim strBodyhead As String = ""
            Dim strbodydetl As String = ""
            Dim strBody As String = ""
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String() = Nothing
            Dim MailAttachmentbytes As New List(Of Byte())()

            strBodyhead = strBodyhead & "<center><span style='font-family:Arial;font-size:X-Large;width:256px;Color:RED'><b>" & cErrMsg & "</b> </span><center>" & vbCrLf
            strBodyhead = strBodyhead & "&nbsp;" & vbCrLf
            strbodydetl = "&nbsp;" & vbCrLf
            strbodydetl = strbodydetl & "<HR width='100%' SIZE='1'>" & vbCrLf
            strbodydetl = strbodydetl & "&nbsp;" & vbCrLf

            strbodydetl = strbodydetl & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf

            strbodydetl = strbodydetl & "<TR>" & vbCrLf
            strbodydetl = strbodydetl & "<TD>" & vbCrLf
            strbodydetl = strbodydetl & "<b>Log File :</b><span> &nbsp;" & m_oLogger.LogFileSpec & " </span></td></tr>"
            strbodydetl = strbodydetl & "<TR>" & vbCrLf
            strbodydetl = strbodydetl & "<TD>" & vbCrLf
            strbodydetl = strbodydetl & "<b>Last Error Message :</b><span> &nbsp;" & sLastErrorMessage & "</span></td></tr>"

            strbodydetl = strbodydetl & "<TD>" & vbCrLf

            strbodydetl = strbodydetl & "<TR>" & vbCrLf

            strbodydetl = strbodydetl & "<TR>" & vbCrLf
            strbodydetl = strbodydetl & "<TD>" & vbCrLf
            strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
            strbodydetl = strbodydetl & "<TR>" & vbCrLf
            strbodydetl = strbodydetl & "<TD>" & vbCrLf
            strbodydetl = strbodydetl & "&nbsp;<br>" & vbCrLf
            strBody = strBodyhead & strbodydetl
            Try
                SDIEmailService.EmailUtilityServices("Mail", "SDIExchADMIN@SDI.com", "WebDev@sdi.com", "Error from Populate_BU_UNSPSC_Tbl", "", "", strBody, "SDIERRMAIL", MailAttachmentName, MailAttachmentbytes.ToArray())

            Catch ex1 As Exception

            End Try
        Catch e As Exception

        End Try
    End Sub

    Private Sub SendEmail(strErrorMessage As String)
        Try
            Const cErrMsg As String = "Erred out UNSPSC code(s) List"
            Dim strBodyhead As String = ""
            Dim strbodydetl As String = ""
            Dim strBody As String = ""
            Dim SDIEmailService As SDiEmailUtilityService.EmailServices = New SDiEmailUtilityService.EmailServices()
            Dim MailAttachmentName As String() = Nothing
            Dim MailAttachmentbytes As New List(Of Byte())()

            strBodyhead = strBodyhead & "<center><span style='font-family:Arial;font-size:X-Large;width:256px;Color:RED'><b>" & cErrMsg & "</b> </span><center>" & vbCrLf
            strBodyhead = strBodyhead & "&nbsp;" & vbCrLf
            strbodydetl = "&nbsp;" & vbCrLf
            strbodydetl = strbodydetl & "<HR width='100%' SIZE='1'>" & vbCrLf
            strbodydetl = strbodydetl & "&nbsp;" & vbCrLf

            strbodydetl = strbodydetl & "<TABLE cellSpacing='1' cellPadding='1' width='100%' border='0'>" & vbCrLf

            strbodydetl = strbodydetl & "<TR>" & vbCrLf
            strbodydetl = strbodydetl & "<TD>" & vbCrLf
            strbodydetl = strbodydetl & "<b>Log File: </b><span> &nbsp;" & m_oLogger.LogFileSpec & " </span></td></tr>"
            strbodydetl = strbodydetl & "<TR>" & vbCrLf
            strbodydetl = strbodydetl & "<TD>" & vbCrLf
            strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
            strbodydetl = strbodydetl & "<TR>" & vbCrLf
            strbodydetl = strbodydetl & "<TD>" & vbCrLf
            strbodydetl = strbodydetl & "<b>Data Source: </b><span> &nbsp;" & Right(ORDBData.DbUrl, 4) & " </span></td></tr>"
            strbodydetl = strbodydetl & "<TR>" & vbCrLf
            strbodydetl = strbodydetl & "<TD>" & vbCrLf
            strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
            strbodydetl = strbodydetl & "<TR>" & vbCrLf
            strbodydetl = strbodydetl & "<TD>" & vbCrLf
            strbodydetl = strbodydetl & "<b>Error(s) List: </b></td></tr>"
            strbodydetl = strbodydetl & "<TR>" & vbCrLf
            strbodydetl = strbodydetl & "<TD>" & vbCrLf
            strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
            strbodydetl = strbodydetl & strErrorMessage

            strbodydetl = strbodydetl & "<TR>" & vbCrLf
            strbodydetl = strbodydetl & "<TD>" & vbCrLf
            strbodydetl = strbodydetl & "<span>&nbsp;</span></td></tr>" & vbCrLf
            'strbodydetl = strbodydetl & "<TR>" & vbCrLf
            'strbodydetl = strbodydetl & "<TD>" & vbCrLf
            strbodydetl = strbodydetl & "&nbsp;<br>" & vbCrLf
            strBody = strBodyhead & strbodydetl
            Dim strSubject As String = " Error from Populate_BU_UNSPSC_Tbl"

            Try
                SDIEmailService.EmailUtilityServices("Mail", "SDIExchADMIN@SDI.com", "WebDev@sdi.com", strSubject, "", "", strBody, "SDIERRMAIL", MailAttachmentName, MailAttachmentbytes.ToArray())

            Catch ex1 As Exception

            End Try
        Catch e As Exception

        End Try
    End Sub

End Module
