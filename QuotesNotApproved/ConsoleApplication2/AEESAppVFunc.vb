Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports ConsoleApplication2.ORDBData
Imports System.IO
Imports System.Security.Cryptography
Imports System.Web.Mail
Imports System.Net
Imports System.Text
Imports System.Web.HttpBrowserCapabilities
Imports System.Threading.Thread
Imports System.Web.UI.WebControls
Imports System.Web.UI.HtmlTextWriter
 

Namespace AEESAppvFunctions

    Public Class AEESAppVFunc

        Public Shared Function GetDisplayName(ByVal name, ByVal strBU, ByVal strUserid) As String
            If IsDBNull(name) Then
                GetDisplayName = GetEmpName(strBU, strUserid)
            ElseIf Trim(name) = "" Then
                GetDisplayName = GetEmpName(strBU, strUserid)
            Else
                GetDisplayName = name
            End If

        End Function

        Public Shared Function GetEmpName(ByVal siteid As String, ByVal userid As String) As String

            Dim strSQLString As String = "Select A.ISA_EMPLOYEE_NAME" & vbCrLf & _
            " from PS_ISA_EMPL_TBL A" & vbCrLf & _
            " where A.ISA_EMPLOYEE_ID = '" & UCase(userid) & "'" & vbCrLf & _
            " AND A.BUSINESS_UNIT = '" & Trim(siteid) & "'" & vbCrLf

            Dim strUserID As String
            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Try
                strUserID = ORDBData.GetScalar(strSQLString)
                Return strUserID
            Catch objException As Exception
                currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
                currentApp.Response.Write("<hr>")
                currentApp.Response.Write("<li>Message: " & objException.Message)
                currentApp.Response.Write("<li>Source: " & objException.Source)
                currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
                currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
                currentApp.Response.End()
            End Try
        End Function

        Public Shared Function getShipToEmpName(ByVal strShipTo) As String

            Dim strSQLstring As String
            strSQLstring = "SELECT A.NAME" & vbCrLf & _
                        " FROM PS_ISA_EMP_LOC_XRF A" & vbCrLf & _
                        " WHERE A.EFFDT =" & vbCrLf & _
                        " (SELECT MAX(A_ED.EFFDT) FROM PS_ISA_EMP_LOC_XRF A_ED" & vbCrLf & _
                        " WHERE A.LOCATION = A_ED.LOCATION" & vbCrLf & _
                        " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                        " AND A.LOCATION = '" & strShipTo & "'" & vbCrLf & _
                        " AND A.EFF_STATUS = 'A'"

            getShipToEmpName = ORDBData.GetScalar(strSQLstring)

        End Function

        Public Shared Function getShipToLocName(ByVal strShipTo) As String

            Dim strSQLstring As String
            strSQLstring = "SELECT A.DESCR" & vbCrLf & _
                            " FROM PS_ISA_SDR_BU_LOC A" & vbCrLf & _
                            " WHERE A.SETID = 'MAIN1'" & vbCrLf & _
                            " AND A.LOCATION = '" & strShipTo & "'"

            getShipToLocName = " - " & ORDBData.GetScalar(strSQLstring)

        End Function

        Public Shared Function GetSiteBU(ByVal sBU) As String

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Dim strSiteBU

            Dim strSQLstring As String
            strSQLstring = "SELECT A.BUSINESS_UNIT" & vbCrLf & _
                " FROM PS_PO_LOADER_DFL A" & vbCrLf & _
                " WHERE SUBSTR(A.LOADER_BU,2) = '" & sBU.Substring(1, 4) & "'" & vbCrLf
            Try
                Dim dtrPrefixReader As OleDbDataReader = ORDBData.GetReader(strSQLstring)
                If dtrPrefixReader.Read() Then
                    strSiteBU = dtrPrefixReader("BUSINESS_UNIT").ToString
                Else
                    dtrPrefixReader.Close()
                    strSQLstring = "SELECT A.TAX_COMPANY" & vbCrLf & _
                                " FROM PS_BUS_UNIT_TBL_OM A" & vbCrLf & _
                                " WHERE A.BUSINESS_UNIT = '" & sBU & "'" & vbCrLf
                    dtrPrefixReader = ORDBData.GetReader(strSQLstring)
                    If dtrPrefixReader.Read() Then
                        strSiteBU = dtrPrefixReader("TAX_COMPANY").ToString
                    End If
                End If
                dtrPrefixReader.Close()

                If Trim(strSiteBU) = "" Then
                    strSiteBU = "ISA00"
                End If

                Return strSiteBU
            Catch objException As Exception
                currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
                currentApp.Response.Write("<hr>")
                currentApp.Response.Write("<li>Message: " & objException.Message)
                currentApp.Response.Write("<li>Source: " & objException.Source)
                currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
                currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
                currentApp.Response.End()

            End Try

        End Function

        Public Shared Function GetSiteName(ByVal sBU) As String
            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            ' Dim strSitePrefix As String
            Dim strSQLString = "SELECT DESCR" & vbCrLf & _
                " FROM PS_BUS_UNIT_TBL_FS" & vbCrLf & _
                " WHERE BUSINESS_UNIT = '" & sBU & "'" & vbCrLf

            GetSiteName = ORDBData.GetScalar(strSQLString)

        End Function

        Public Shared Function GetUserInfo(ByVal strBU, ByVal strEmpID) As DataSet

            Dim strSQLstring As String
            strSQLstring = "SELECT A.ISA_EMPLOYEE_EMAIL, A.PHONE_NUM" & vbCrLf & _
                    " FROM PS_ISA_USERS_TBL A" & vbCrLf & _
                    " WHERE BUSINESS_UNIT = '" & strBU & "'" & vbCrLf & _
                    " AND ISA_EMPLOYEE_ID = '" & strEmpID & "'"

            GetUserInfo = ORDBData.GetAdapter(strSQLstring)

        End Function

        Public Shared Sub SetupSessionVars()
            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            currentApp.Session("BUSUNIT") = currentApp.Request.QueryString("1")
            currentApp.Session("AGENTUSERBU") = currentApp.Request.QueryString("2")
            currentApp.Session("USERID") = currentApp.Request.QueryString("3")
            currentApp.Session("AGENTUSERID") = currentApp.Request.QueryString("4")
            currentApp.Session("CONAME") = currentApp.Request.QueryString("5")
            If currentApp.Session("CONAME") Is Nothing Then
                Dim objenterprise As New clsEnterprise(currentApp.Session("BUSUNIT"))
                Dim strCustID As String = objenterprise.CustID
                Dim objcustomer As New clsCustomer(strCustID)
                currentApp.Session("CONAME") = objcustomer.Name1
            End If
            currentApp.Session("USERNAME") = currentApp.Request.QueryString("6")
            currentApp.Session("AGENTUSERNAME") = currentApp.Request.QueryString("7")
            currentApp.Session("SDIEMP") = currentApp.Request.QueryString("8")
            currentApp.Session("HOME1") = currentApp.Request.QueryString("9")

            If currentApp.Session("BUSUNIT") = "SDI00" Then
                currentApp.Session.Timeout = 120
            End If
        End Sub

        Public Shared Sub sendemail(ByVal mailer As MailMessage)

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance
            Dim connectionEmail = New OleDbConnection(DbUrl)
            Try
                If currentApp.Request.ServerVariables("HTTP_HOST").ToString().ToUpper = "LOCALHOST" Then
                    SmtpMail.SmtpServer = "127.0.0.1"
                End If
                'SmtpMail.Send(mailer)
                UpdEmailOut.UpdEmailOut.UpdEmailOut(mailer.Subject, _
                                                mailer.From, _
                                                mailer.To, _
                                                mailer.Cc, _
                                                mailer.Bcc, _
                                                "N", _
                                                mailer.Body, _
                                                connectionEmail)
            Catch ex As Exception
                currentApp.Response.Write(("The following exception occurred: " + ex.ToString()))
                'check the InnerException
                While Not (ex.InnerException Is Nothing)
                    currentApp.Response.Write("--------------------------------")
                    currentApp.Response.Write(("The following InnerException reported: " + ex.InnerException.ToString()))
                    ex = ex.InnerException
                End While
            End Try
        End Sub

        Public Shared Sub WebLog()

            'Gives us a reference to the current asp.net 
            'application executing the method.
            Dim currentApp As System.Web.HttpApplication = System.Web.HttpContext.Current.ApplicationInstance

            If currentApp.Session("USERID") = "" Then
                currentApp.Session("USERID") = "EMPTY"
            End If

            If IsDBNull(currentApp.Session("BUSUNIT")) Or _
                currentApp.Session("BUSUNIT") Is Nothing Then
                Exit Sub
            End If

            'Dim strSQLstring As String = "INSERT INTO ps_isa_web_log" & vbCrLf & _
            '        " ( BUSINESS_UNIT, ISA_EMPLOYEE_ID, ISA_EMPLOYEE_NAME," & vbCrLf & _
            '        " ISA_SDI_EMPLOYEE, ISA_LOG_PAGE, ISA_LOG_URL," & vbCrLf & _
            '        " ISA_LOG_IP, ISA_LOG_BROWSER, ISA_LOG_COOKIES," & vbCrLf & _
            '        " ISA_LOG_JAVASCRIPT, ISA_LOG_JAVAAPPLET," & vbCrLf & _
            '        " ISA_LOG_VBSCRIPT, ISA_LOG_ACTIVEX," & vbCrLf & _
            '        " ISA_LOG_PLATFORM, DT_TIMESTAMP )" & vbCrLf & _
            '        " VALUES ( '" & currentApp.Session("BUSUNIT") & "'," & vbCrLf & _
            '        " '" & currentApp.Session("USERID") & "'," & vbCrLf & _
            '        " '" & currentApp.Session("USERNAME") & "'," & vbCrLf & _
            '        " '" & Left(currentApp.Session("SDIEMP"), 1) & "'," & vbCrLf & _
            '        " '" & Left(currentApp.Request.ServerVariables("SCRIPT_NAME"), 60) & "'," & vbCrLf & _
            '        " '" & Left((currentApp.Request.ServerVariables("HTTP_HOST") & currentApp.Request.ServerVariables("SCRIPT_NAME")), 100) & "'," & vbCrLf & _
            '        " '" & currentApp.Request.ServerVariables("REMOTE_ADDR") & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.Browser & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.Cookies & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.JavaScript & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.JavaApplets & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.VBScript & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.ActiveXControls & "'," & vbCrLf & _
            '        " '" & currentApp.Request.Browser.Platform & "'," & vbCrLf & _
            '        " TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM'))"

            'Dim rowsaffected As Integer

            'Try
            '    Dim connection = New OleDbConnection(DbUrl)
            '    Dim Command = New OleDbCommand(strSQLstring, connection)
            '    connection.open()
            '    rowsaffected = Command.ExecuteNonQuery()
            '    connection.close()
            'Catch objException As Exception
            '    'currentApp.Response.Write("We're sorry, we are experiencing technical problems...")
            '    'currentApp.Response.Write("<hr>")
            '    'currentApp.Response.Write("<li>Message: " & objException.Message)
            '    'currentApp.Response.Write("<li>Source: " & objException.Source)
            '    'currentApp.Response.Write("<li>Stack Trace: " & objException.StackTrace)
            '    'currentApp.Response.Write("<li>Target Site: " & objException.TargetSite.Name)
            '    'currentApp.Response.Write("<li>SQL: " & p_strQuery)
            '    'currentApp.Response.End()
            'End Try

            'Sleep(1000)

        End Sub

    End Class

End Namespace
