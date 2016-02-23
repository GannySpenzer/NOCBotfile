Imports System
Imports System.Data
Imports System.Web
Imports System.IO
Imports System.Data.OleDb
Imports System.Web.Mail
Imports System.Web.UI
Imports System.Text
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.Xml.Serialization
Imports System.Data.SqlClient
Imports System.Math

Module Module1
    Dim ORDB As String = "PROD"
    'Dim SQLDB As String = "CONTENTPLUS_JR"
    Dim SQLDB As String = "CPLUS_PROD"
    Dim SQLDBPROD As String = "DAZZLE"
    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\Incident_Vendor"
    Dim logpath As String = "C:\Incident_Vendor\LOGS\Incident_Vendor" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=einternet;User ID=einternet;Data Source=RPTG")
    'Dim connectCplus As New SqlConnection("server=" & SQLDB & ";uid=einternet;pwd=einternet;Initial Catalog=ContentPlus;Data Source=" & SQLDB & "")
    Dim connectCplus = "server=DAZZLE2;uid=sa;pwd=sdiadmin;initial catalog=pubs;"
    'Dim connectionOS As New OleDbConnection(ORDBData.DbUrl)

    Sub Main()
        Console.WriteLine("Start Incident_Vendor Build")
        Console.WriteLine("")

        'If Dir(rootDir, FileAttribute.Directory) = "" Then
        '    MkDir(rootDir)
        'End If
        'If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
        '    MkDir(rootDir & "\LOGS")
        'End If

        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("Building Incident Vendors out " & Now())

        Dim bolError As Boolean = buildIncidentVendors()

        If bolError = True Then
            SendErrEmail()
        End If
        objStreamWriter.WriteLine("End of sendCustEmails Email send " & Now())

        objStreamWriter.Flush()
        objStreamWriter.Close()
    End Sub
    Private Function buildIncidentVendors() As Boolean

        Dim strSQLstring As String
        Dim rowsaffected As Integer
        Dim Command1 As New OleDbCommand

     
        'strSQLstring = "" & _
        '                    " SELECT Distinct A.vendor_id, b.name1" & vbCrLf & _
        '                    " FROM ps_po_hdr A, PS_VENDOR B" & vbCrLf & _
        '                    " WHERE where sysdate > PO_DT - 720 " & vbCrLf & _
        '                    " and A.vendor_id = B.vendor_ID order by name1  " & vbCrLf
        Dim commandOR As New OleDbCommand(" SELECT Distinct A.vendor_id, b.name1" & vbCrLf & _
                            " FROM ps_po_hdr A, PS_VENDOR B" & vbCrLf & _
                            " WHERE trunc(PO_DT) > trunc(sysdate) - 730 " & vbCrLf & _
                            " and A.vendor_id = B.vendor_ID order by name1  ", connectOR)

        ' Dim Command As OleDbCommand = New OleDbCommand(strSQLstring, connectOR)
        connectOR.Open()
        Dim dataAdapter As New OleDbDataAdapter(commandOR)
        Dim ds As System.Data.DataSet = New System.Data.DataSet
        Try
            dataAdapter.Fill(ds)
            connectOR.Close()
        Catch ex As Exception
            objStreamWriter.WriteLine("     error selecting from PS_PO_Hdr or PS_Vendor tables")
            objStreamWriter.WriteLine("         " & ex.Message)
            connectOR.Close()
            Return True
        End Try

        If ds.Tables(0).Rows.Count = 0 Then
            objStreamWriter.WriteLine("  Vendors Selected = 0")
            connectOR.Close()
            Return False
        End If
        connectOR.Close()

        Console.WriteLine("Vendors Selected = " & ds.Tables(0).Rows.Count.ToString())
        ' delete all the records first then
        'refresh table - insert into the Incident_Vendors (in Dazzle/pubs) table

        Dim I As Integer
        Dim bolEmailSent As Boolean
        Dim cn As New SqlConnection(connectCplus)

        Dim strItemID As String = " "
        Dim strItemName As String = " "
        cn.Open()
        strSQLstring = "delete Incident_Vendors " & vbCrLf
        Try

        Catch exSQL As SqlException
            objStreamWriter.WriteLine("     error connecting to database")
            objStreamWriter.WriteLine("         " & exSQL.Message)
            connectOR.Close()
            Return True
        End Try
        If Not (cn.State = ConnectionState.Open) Then
            objStreamWriter.WriteLine("     error connecting to database")
            objStreamWriter.WriteLine("         ")
            connectOR.Close()
            Return True
        End If

        Try
            Dim Command13 As New SqlCommand(strSQLstring, cn)
            rowsaffected = Command13.ExecuteNonQuery
            Command13.Dispose()
            Console.WriteLine("Finished deleting")
            objStreamWriter.WriteLine("Finished deleting")
        Catch ex As Exception
            objStreamWriter.WriteLine("  nothing deleted")
            objStreamWriter.WriteLine(ex.ToString)
            Return True
        End Try
         

        For I = 0 To ds.Tables(0).Rows.Count - 1
            Try
                strItemID = ds.Tables(0).Rows(I).Item("vendor_id")
                strItemName = ds.Tables(0).Rows(I).Item("name1")
                strItemName = Replace(strItemName, "'", " ")
            Catch ex As Exception

            End Try


            strSQLstring = "Insert into Incident_Vendors " & vbCrLf & _
                                   " (Incident_Vendor_ID," & vbCrLf & _
                                   " Incident_Vendor_name) " & vbCrLf & _
                                   " VALUES('" & strItemID & "'," & vbCrLf & _
                                   " '" & strItemName & "')"



            'Try

            'Catch exSQL As SqlException
            '    objStreamWriter.WriteLine("     error connecting to database")
            '    objStreamWriter.WriteLine("         " & exSQL.Message)
            '    connectOR.Close()
            '    Return True
            'End Try
            If Not (cn.State = ConnectionState.Open) Then
                Return True
            End If

            Try
                If IsDBNull(strItemID) Or IsDBNull(strItemName) Then
                    rowsaffected = 1
                Else
                    Dim Command12 As New SqlCommand(strSQLstring, cn)
                    rowsaffected = Command12.ExecuteNonQuery
                    Command12.Dispose()
                End If
                If Not rowsaffected = 1 Then
                    objStreamWriter.WriteLine("  Incident_Vendor_load Vendor for " & ds.Tables(0).Rows(I).Item("vendor_id") & " " & _
                     ds.Tables(0).Rows(I).Item("name1"))

                End If

            Catch ex As Exception
                objStreamWriter.WriteLine("  Incident_Vendor_load Vendor Error for " & ds.Tables(0).Rows(I).Item("vendor_id") & " " & _
                     ds.Tables(0).Rows(I).Item("name1"))
                objStreamWriter.WriteLine(ex.ToString)

            End Try

            If I = 200 Then
                Console.WriteLine("Processed row number 200")
                objStreamWriter.WriteLine("Processed row number 200")
            End If
            If I = 450 Then
                Console.WriteLine("Processed row number 450")
                objStreamWriter.WriteLine("Processed row number 450")
            End If
            If I = 720 Then
                Console.WriteLine("Processed row number 720")
                objStreamWriter.WriteLine("Processed row number 720")
            End If
            If I = 1000 Then
                Console.WriteLine("Processed row number 1000")
                objStreamWriter.WriteLine("Processed row number 1000")
            End If
            If I = 2000 Then
                Console.WriteLine("Processed row number 2000")
                objStreamWriter.WriteLine("Processed row number 2000")
            End If
            If I = 3000 Then
                Console.WriteLine("Processed row number 3000")
                objStreamWriter.WriteLine("Processed row number 3000")
            End If
            If I = 4000 Then
                Console.WriteLine("Processed row number 4000")
                objStreamWriter.WriteLine("Processed row number 4000")
            End If
            If I = 5000 Then
                Console.WriteLine("Processed row number 5000")
                objStreamWriter.WriteLine("Processed row number 5000")
            End If
            If I = 6000 Then
                Console.WriteLine("Processed row number 6000")
                objStreamWriter.WriteLine("Processed row number 6000")
            End If
            If I = 7000 Then
                Console.WriteLine("Processed row number 7000")
                objStreamWriter.WriteLine("Processed row number 7000")
            End If
            If I = 8000 Then
                Console.WriteLine("Processed row number 8000")
                objStreamWriter.WriteLine("Processed row number 8000")
            End If
            If I = 9000 Then
                Console.WriteLine("Processed row number 9000")
                objStreamWriter.WriteLine("Processed row number 9000")
            End If

        Next

        Command1.Dispose()

        objStreamWriter.WriteLine("  vendors inserted into Incident_Vendors = " & ds.Tables(0).Rows.Count)

        Try
            cn.Close()
            cn.Dispose()
        Catch ex As Exception
        Finally
            cn = Nothing
        End Try
    End Function
    Private Sub SendErrEmail()

        Dim email As New MailMessage
        email.From = "TechSupport@sdi.com"
        email.To = "vitaly.rovensky@sdi.com"
        email.Subject = "Sent Incident Vendor Build Error"
        email.Priority = MailPriority.High
        email.BodyFormat = MailFormat.Html
        email.Body = "<html><body><table><tr><td>Incident Vendor Build has completed with errors, review log.</td></tr>"

        'Send the email and handle any error that occurs
        Try
            'SmtpMail.Send(email)

            SendEmail1(email)
        Catch
            objStreamWriter.WriteLine("     Error - the email was not sent")
        End Try

    End Sub


    Private Sub SendEmail1(ByVal mailer As System.Web.Mail.MailMessage)

        Try

            UpdEmailOut.UpdEmailOut.UpdEmailOut(mailer.Subject, mailer.From, mailer.To, mailer.Cc, mailer.Bcc, "N", mailer.Body, connectOR)
        Catch ex As Exception

        End Try
    End Sub


End Module
