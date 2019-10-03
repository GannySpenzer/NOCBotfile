Imports System.IO
Imports System.Data
Imports System.ServiceProcess
'Imports System.Text.StringBuilder
Imports System.Threading.Thread
Imports System.Diagnostics


Public Class SiteUpload

    Dim strServerFolder As String = ""
    Dim strSQLString As String = ""
    Dim dsTable As DataSet
    Dim strTargetDir As String = ""
    Dim copyFrom As String = ""
    Dim copyTo As String = ""
    'Dim doneReplace As Boolean = False

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load

        strSQLString = "SELECT * FROM SDIX_DEPLOY_SITES"
        dsTable = ORDBData.GetAdapter(strSQLString)

        Dim dt As DataTable = dsTable.Tables(0)
        Dim query As String() = (From row In dt.AsEnumerable() Select (row("SERVER_NAME").ToString())).Distinct.ToArray()

        For Each server In query
            cmbServer.Items.Add(server.ToString)
        Next server
        cmbServer.SelectedIndex = 0

        Me.CenterToScreen()

        cmbServer.DropDownStyle = ComboBoxStyle.DropDownList
        cmbSites.DropDownStyle = ComboBoxStyle.DropDownList

    End Sub

    Private Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
        FolderBrowserDialog1.SelectedPath = txtSource.Text
        FolderBrowserDialog1.ShowDialog()

        If DialogResult.OK Then
            txtSource.Text = FolderBrowserDialog1.SelectedPath.ToString
        End If

    End Sub

    Private Sub btnGo_Click(sender As Object, e As EventArgs) Handles btnGo.Click
        Dim strMessage As String = ""
        Dim strServer As String = cmbServer.Text

        Try
            Application.UseWaitCursor = True

            'Dim filesInFolder = Directory.GetFiles(txtSource.Text, "*.*", SearchOption.AllDirectories).Count
            'MsgBox(filesInFolder)
            'Exit Sub

            If cmbServer.Text = "SDIX2012" Or cmbServer.Text = "SDIX2012ZEUS2" Then
                ProcessProd()
                Exit Sub
            End If

            strMessage = "Will performing the following tasks:" & vbCrLf & vbCrLf
            strMessage += "1. Copy \\{0}\sdi.pickingreports.dll.config to " & vbCrLf & txtSource.Text & "\bin\" & vbCrLf & vbCrLf
            strMessage += "2. Copy \\{0}\web.config to " & vbCrLf & txtSource.Text & vbCrLf & vbCrLf
            strMessage += "3. Delete everything in \\{0}\c$\inetpub\wwwroot\{1}" & vbCrLf & vbCrLf
            strMessage += "4. Copy all content of " & txtSource.Text & " to \\{0}\c$\inetpub\wwwroot\{1}" & vbCrLf & vbCrLf
            strMessage += "5. Update SDIX_DEPLOY_SITES with new deployment information"
            Dim strBuilder As New System.Text.StringBuilder

            lblMessage.Clear()
            lblMessage.ForeColor = Color.Black

            'validation
            If txtSource.Text.Trim = "" Then
                lblMessage.Text = "Select a source directory first."
                lblMessage.ForeColor = Color.Red
                Exit Sub
            Else
                Try
                    Dim trySrc As New DirectoryInfo(txtSource.Text)
                    If Not trySrc.Exists Then
                        lblMessage.Text = "Source directory not found."
                        lblMessage.ForeColor = Color.Red
                        Exit Sub
                    End If
                Catch ex As Exception
                    lblMessage.Text = "Source directory not found."
                    lblMessage.ForeColor = Color.Red
                    Exit Sub
                End Try
            End If
            If lblWebConfigSrv.Text = "NOT FOUND" Then
                lblMessage.Text = "Select a valid web.config first."
                lblMessage.ForeColor = Color.Red
                Exit Sub
            End If

            Try
                strMessage = strBuilder.AppendFormat(strMessage, cmbServer.Text, strServerFolder).ToString
                Dim result As Integer = MessageBox.Show(strMessage, "Import data", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation)
                If result = DialogResult.Cancel Then
                    Exit Sub
                End If

                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                'Exit Sub
                btnGo.Enabled = False

                Try

                    copyFrom = "\\" & cmbServer.Text & "\c$\copyforvr\backup\sdi.pickingreports.dll.config"
                    copyTo = txtSource.Text & "\bin\sdi.pickingreports.dll.config"
                    lblMessage.Text = "Copying " & copyFrom & " to " & copyTo & "..." & vbCrLf
                    Me.Refresh()
                    Application.DoEvents()
                    My.Computer.FileSystem.CopyFile(copyFrom, copyTo, True)
                Catch ex As Exception
                    lblMessage.Text = ex.ToString
                    lblMessage.ForeColor = Color.Red
                    Exit Sub
                End Try

                Try
                    copyFrom = "\\" & cmbServer.Text & "\c$\copyforvr\backup\" & strServerFolder & "\web.config"
                    copyTo = txtSource.Text & "\web.config"
                    'lblMessage.Text += "Copying " & copyFrom & " to " & copyTo & "..." & vbCrLf
                    lblMessage.AppendText("Copying " & copyFrom & " to " & copyTo & "..." & vbCrLf)
                    Me.Refresh()
                    Application.DoEvents()
                    My.Computer.FileSystem.CopyFile(copyFrom, copyTo, True)
                Catch ex As Exception
                    lblMessage.Text = ex.ToString
                    lblMessage.ForeColor = Color.Red
                    Exit Sub
                End Try

                Try
                    Dim delDir As String = "\\" & cmbServer.Text & "\c$\inetpub\wwwroot\" & strServerFolder
                    'lblMessage.Text += "Deleting contents of " & delDir & "..." & vbCrLf
                    lblMessage.AppendText("Deleting contents of " & delDir & "..." & vbCrLf)

                    Dim cnt As Integer
                    cnt = Directory.GetFiles(delDir, "*.*", SearchOption.TopDirectoryOnly).Count
                    cnt += Directory.GetDirectories(delDir).Count
                    ProgressBar1.Maximum = cnt 'Directory.GetFiles(delDir, "*.*", SearchOption.AllDirectories).Count
                    ProgressBar1.Step = 1
                    ProgressBar1.Value = 0
                    Dim delFilesCount As Integer = 0
                    lblProgress.Visible = True

                    result = MessageBox.Show("Delete contents of " & delDir & " or cancel processing?", "Delete target directory", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation)
                    If result = DialogResult.Cancel Then
                        lblMessage.Text = "Processing incomplete.  Delete canceled."
                        lblMessage.ForeColor = Color.Red
                        Exit Sub
                    End If
                    Me.Refresh()
                    Application.DoEvents()
                    Process.Start("\\" & cmbServer.Text & "\c$\inetpub\wwwroot\" & strServerFolder) 'show folder!

                    For Each di In Directory.GetDirectories(delDir)
                        Directory.Delete(di, True)
                        delFilesCount += 1
                        ProgressBar1.Value = delFilesCount
                        lblProgress.Text = delFilesCount & " of " & cnt & " total files/folders deleted."
                        Me.Refresh()
                        Application.DoEvents()
                    Next
                    For Each delFile In Directory.GetFiles(delDir, "*.*")
                        File.Delete(delFile)
                        delFilesCount += 1
                        ProgressBar1.Value = delFilesCount
                        lblProgress.Text = delFilesCount & " of " & cnt & " total files/folders deleted."
                        Me.Refresh()
                        Application.DoEvents()
                    Next
                Catch ex As Exception
                    lblMessage.Text = ex.ToString
                    lblMessage.ForeColor = Color.Red
                    Exit Sub
                End Try

                Try

                    copyFrom = txtSource.Text
                    copyTo = "\\" & cmbServer.Text & "\c$\inetpub\wwwroot\" & strServerFolder
                    'lblMessage.Text += "Copying " & copyFrom & " to " & copyTo & "..." & vbCrLf
                    lblMessage.AppendText("Copying " & copyFrom & " to " & copyTo & "..." & vbCrLf)

                    strTargetDir = copyFrom
                    Dim cnt As Integer
                    cnt = Directory.GetFiles(strTargetDir, "*.*", SearchOption.AllDirectories).Count '100
                    ProgressBar1.Maximum = cnt
                    ProgressBar1.Step = 1
                    ProgressBar1.Value = 0
                    BackgroundWorker1.RunWorkerAsync()

                    Timer1.Enabled = True

                    Me.Refresh()
                    Application.DoEvents()
                    'My.Computer.FileSystem.CopyDirectory(copyFrom, copyTo, True)
                Catch ex As Exception
                    lblMessage.Text = ex.ToString
                    lblMessage.ForeColor = Color.Red
                    Exit Sub
                End Try

                'Try
                '    'db name, description, deploy date
                '    lblMessage.Text += "Updating SDIX_DEPLOY_SITES table" & vbCrLf
                '    Me.Refresh()
                '    Application.DoEvents()
                '    strSQLString = "UPDATE SDIX_DEPLOY_SITES SET "
                '    strSQLString += " DB_NAME = '" & lblWebConfigSrv.Text & "',"
                '    strSQLString += " DEPLOY_DATE = SYSDATE"
                '    If txtDescription.Text.Trim <> "" Then
                '        txtDescription.Text = Replace(txtDescription.Text, "'", "''")
                '        strSQLString += ",DESCRIPTION = '" & txtDescription.Text & "'"
                '    End If
                '    strSQLString += " WHERE SITE_NAME = '" & cmbSites.Text & "'"
                '    iResponse = ORDBData.ExecNonQuery(strSQLString)
                'Catch ex As Exception
                '    lblMessage.Text = ex.ToString
                '    lblMessage.ForeColor = Color.Red
                '    Exit Sub
                'End Try
                ' '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

                'lblMessage.ForeColor = Color.Green
                'lblMessage.Text += "Process complete!"

            Catch ex As Exception
                lblMessage.Text = ex.ToString
                lblMessage.ForeColor = Color.Red

            End Try

        Catch ex As Exception

        Finally
            Application.UseWaitCursor = False
            btnGo.Enabled = True
        End Try

    End Sub

    Public Sub ProcessProd()
        Dim strMessage As String = ""
        Dim strServer As String = cmbServer.Text
        Dim strConfigSource As String = ""

        Select Case cmbServer.Text
            Case "SDIX2012"
                strConfigSource = System.Configuration.ConfigurationSettings.AppSettings("SDIXProdConfigFiles")
            Case "SDIX2012ZEUS2"
                strConfigSource = System.Configuration.ConfigurationSettings.AppSettings("ZeusProdConfigFiles")
        End Select

        Dim strProdUpdateFiles As String() = New String(7) {}
        strProdUpdateFiles(0) = "printJob.template.xml"
        strProdUpdateFiles(1) = "SDI.PickingReports.dll.config"
        strProdUpdateFiles(2) = "SDI.PrintJob.dll.config"
        strProdUpdateFiles(3) = "SDI.PrintJob.nycPreDelivForm.dll.config"
        strProdUpdateFiles(4) = "SDI.PrintJob.nycPreDelivForm.template.xml"
        strProdUpdateFiles(5) = "SDI.PrintJob.nycTruckingDelivForm.dll.config"
        strProdUpdateFiles(6) = "SDI.PrintJob.nycTruckingDelivForm.template.xml"
        strProdUpdateFiles(7) = "UpdEmailOut.dll.config"

        Try

            strMessage = "Will performing the following tasks:" & vbCrLf & vbCrLf
            strMessage += "1. Copy {0}\web.config to " & vbCrLf & txtSource.Text & vbCrLf & vbCrLf
            strMessage += "2. Copy {0}\Punchout.xml to " & vbCrLf & txtSource.Text & "\PunchOutcXML" & vbCrLf & vbCrLf

            strMessage += "3. Copy {0}\" & vbCrLf
            For i = 0 To 7
                strMessage += strProdUpdateFiles(i) + " to " & vbCrLf & txtSource.Text & "\bin" & vbCrLf
            Next i
            strMessage += vbCrLf

            strMessage += "4. STOP IIS Services with a 3 second pause" & vbCrLf & vbCrLf
            strMessage += "5. Delete everything in " & cmbServer.Text & "\c$\inetpub\wwwroot\{1}" & vbCrLf & vbCrLf
            strMessage += "6. Copy all content of " & cmbServer.Text & " to " & txtSource.Text & "\c$\inetpub\wwwroot\{1}" & vbCrLf & vbCrLf
            strMessage += "7. START IIS Services with a 3 second pause" & vbCrLf & vbCrLf
            strMessage += "8. Update SDIX_DEPLOY_SITES with new deployment information"
            Dim strBuilder As New System.Text.StringBuilder

            lblMessage.Clear()
            lblMessage.ForeColor = Color.Black

            'validation
            If txtSource.Text.Trim = "" Then
                lblMessage.Text = "Select a source directory first."
                lblMessage.ForeColor = Color.Red
                Exit Sub
            Else
                Try
                    Dim trySrc As New DirectoryInfo(txtSource.Text)
                    If Not trySrc.Exists Then
                        lblMessage.Text = "Source directory not found."
                        lblMessage.ForeColor = Color.Red
                        Exit Sub
                    End If
                Catch ex As Exception
                    lblMessage.Text = "Source directory not found."
                    lblMessage.ForeColor = Color.Red
                    Exit Sub
                End Try
            End If
            If lblWebConfigSrv.Text = "NOT FOUND" Then
                lblMessage.Text = "Select a valid web.config first."
                lblMessage.ForeColor = Color.Red
                Exit Sub
            End If

            Try
                strMessage = strBuilder.AppendFormat(strMessage, strConfigSource, cmbServer.Text, strServerFolder).ToString
                Dim result As Integer = MessageBox.Show(strMessage, "Import data", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation)
                If result = DialogResult.Cancel Then
                    Exit Sub
                End If

                ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
                'Exit Sub
                btnGo.Enabled = False

                Try
                    copyFrom = strConfigSource & "\web.config"
                    copyTo = txtSource.Text & "\web.config"
                    'lblMessage.Text += "Copying " & copyFrom & " to " & copyTo & "..." & vbCrLf
                    lblMessage.AppendText("Copying " & copyFrom & " to " & copyTo & "..." & vbCrLf)
                    Me.Refresh()
                    Application.DoEvents()
                    Dim strtestFromDate As String = System.IO.File.GetLastWriteTime(copyFrom)
                    My.Computer.FileSystem.CopyFile(copyFrom, copyTo, True)

                    Dim strtestToDate As String = System.IO.File.GetLastWriteTime(copyTo)


                    If Not My.Computer.FileSystem.FileExists(copyTo) Or strtestFromDate <> strtestToDate Then
                        lblMessage.Text = copyTo + " was not copied.  Stopping process"
                        lblMessage.ForeColor = Color.Red
                        Exit Sub
                    End If

                Catch ex As Exception
                    lblMessage.Text = ex.ToString
                    lblMessage.ForeColor = Color.Red
                    Exit Sub
                End Try

                'Exit Sub

                Try

                    'copyFrom = "\\" & cmbServer.Text & "\c$\copyforvr\backup\sdi.pickingreports.dll.config"
                    copyFrom = strConfigSource & "\Punchout.xml"
                    copyTo = txtSource.Text & "\PunchoutcXML\Punchout.xml"
                    lblMessage.Text = "Copying " & copyFrom & " to " & copyTo & "..." & vbCrLf
                    Me.Refresh()
                    Application.DoEvents()
                    Dim strtestFromDate As String = System.IO.File.GetLastWriteTime(copyFrom)
                    My.Computer.FileSystem.CopyFile(copyFrom, copyTo, True)

                    Dim strtestToDate As String = System.IO.File.GetLastWriteTime(copyTo)

                    If Not My.Computer.FileSystem.FileExists(copyTo) Or strtestFromDate <> strtestToDate Then
                        lblMessage.Text = copyTo + " was not copied.  Stopping process"
                        lblMessage.ForeColor = Color.Red
                        Exit Sub
                    End If

                Catch ex As Exception
                    lblMessage.Text = ex.ToString
                    lblMessage.ForeColor = Color.Red
                    Exit Sub
                End Try

                Try
                    For i = 0 To 7
                        copyFrom = strConfigSource & "\" & strProdUpdateFiles(i)
                        copyTo = txtSource.Text & "\Bin\" & strProdUpdateFiles(i)
                        lblMessage.AppendText("Copying " & copyFrom & " to " & copyTo & "..." & vbCrLf)
                        Me.Refresh()
                        Application.DoEvents()
                        Dim strtestFromDate As String = System.IO.File.GetLastWriteTime(copyFrom)
                        My.Computer.FileSystem.CopyFile(copyFrom, copyTo, True)

                        Dim strtestToDate As String = System.IO.File.GetLastWriteTime(copyTo)

                        If Not My.Computer.FileSystem.FileExists(copyTo) Or strtestFromDate <> strtestToDate Then
                            lblMessage.Text = copyTo + " was not copied.  Stopping process"
                            lblMessage.ForeColor = Color.Red
                            Exit Sub
                        End If
                    Next

                Catch ex As Exception
                    lblMessage.Text = ex.ToString
                    lblMessage.ForeColor = Color.Red
                    Exit Sub
                End Try

                'Exit Sub

                'Sleep(3000)
                lblMessage.AppendText("STOPPING IIS" & vbCrLf)
                Me.Refresh()
                Application.DoEvents()
                'System.Diagnostics.Process.Start("iisreset /stop") 'Stop IIS
                'Sleep(3000)

                Dim stopinfo As New ProcessStartInfo("iisreset.exe", "/stop")
                stopinfo.WindowStyle = ProcessWindowStyle.Hidden
                stopinfo.Verb = "runas"
                'Process.Start(stopinfo)
                Dim p1 As New Process
                p1.StartInfo = stopinfo
                p1.Start()
                p1.WaitForExit()
                p1.Close()

                Try
                    Dim delDir As String = "\\" & cmbServer.Text & "\c$\inetpub\wwwroot\" & strServerFolder
                    lblMessage.AppendText("Deleting contents of " & delDir & "..." & vbCrLf)

                    Dim cnt As Integer
                    cnt = Directory.GetFiles(delDir, "*.*", SearchOption.TopDirectoryOnly).Count
                    cnt += Directory.GetDirectories(delDir).Count
                    ProgressBar1.Maximum = cnt 'Directory.GetFiles(delDir, "*.*", SearchOption.AllDirectories).Count
                    ProgressBar1.Step = 1
                    ProgressBar1.Value = 0
                    Dim delFilesCount As Integer = 0
                    lblProgress.Visible = True

                    result = MessageBox.Show("Delete contents of " & delDir & " or cancel processing?", "Delete target directory", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation)
                    If result = DialogResult.Cancel Then
                        lblMessage.Text = "Processing incomplete.  Delete canceled."
                        lblMessage.ForeColor = Color.Red
                        Exit Sub
                    End If
                    Me.Refresh()
                    Application.DoEvents()
                    Process.Start("\\" & cmbServer.Text & "\c$\inetpub\wwwroot\" & strServerFolder) 'show folder!

                    For Each di In Directory.GetDirectories(delDir)
                        Directory.Delete(di, True)
                        delFilesCount += 1
                        ProgressBar1.Value = delFilesCount
                        lblProgress.Text = delFilesCount & " of " & cnt & " total files/folders deleted."
                        Me.Refresh()
                        Application.DoEvents()
                    Next
                    For Each delFile In Directory.GetFiles(delDir, "*.*")
                        File.SetAttributes(delFile, FileAttributes.Normal)
                        File.Delete(delFile)
                        delFilesCount += 1
                        ProgressBar1.Value = delFilesCount
                        lblProgress.Text = delFilesCount & " of " & cnt & " total files/folders deleted."
                        Me.Refresh()
                        Application.DoEvents()
                    Next
                Catch ex As Exception
                    lblMessage.Text = ex.ToString
                    lblMessage.ForeColor = Color.Red
                    Exit Sub
                End Try

                Try
                    copyFrom = txtSource.Text
                    copyTo = "\\" & cmbServer.Text & "\c$\inetpub\wwwroot\" & strServerFolder
                    'lblMessage.Text += "Copying " & copyFrom & " to " & copyTo & "..." & vbCrLf
                    lblMessage.AppendText("Copying " & copyFrom & " to " & copyTo & "..." & vbCrLf)

                    strTargetDir = copyFrom
                    Dim cnt As Integer
                    cnt = Directory.GetFiles(strTargetDir, "*.*", SearchOption.AllDirectories).Count '100
                    ProgressBar1.Maximum = cnt
                    ProgressBar1.Step = 1
                    ProgressBar1.Value = 0
                    BackgroundWorker1.RunWorkerAsync()

                    Timer1.Enabled = True

                    Me.Refresh()
                    Application.DoEvents()
                    'My.Computer.FileSystem.CopyDirectory(copyFrom, copyTo, True)
                Catch ex As Exception
                    lblMessage.Text = ex.ToString
                    lblMessage.ForeColor = Color.Red
                    Exit Sub
                End Try

                Dim ss As System.IO.StreamWriter
                ss = My.Computer.FileSystem.OpenTextFileWriter("readyforiisstart.txt", True)
                ss.WriteLine("Done.")
                ss.Close()

                Do
                    If My.Computer.FileSystem.FileExists("readyforiisstart.txt") Then
                        My.Computer.FileSystem.DeleteFile("readyforiisstart.txt")
                        Exit Do
                    End If
                    Sleep(1000)
                Loop

                lblMessage.AppendText("RESTARTING IIS" & vbCrLf)
                Me.Refresh()
                Application.DoEvents()
                'System.Diagnostics.Process.Start("iisreset /start") 'Restart IIS
                'Sleep(3000)

                Dim startinfo As New ProcessStartInfo("iisreset.exe", "/start")
                stopinfo.WindowStyle = ProcessWindowStyle.Hidden
                startinfo.Verb = "runas"
                Dim p2 As New Process
                p2.StartInfo = startinfo
                p2.Start()
                p2.WaitForExit()
                p2.Close()

                lblMessage.ForeColor = Color.Green
                lblMessage.Text += "Prod upload complete!"

            Catch ex As Exception
                lblMessage.Text = ex.ToString
                lblMessage.ForeColor = Color.Red

            End Try

        Catch ex As Exception

        Finally
            Application.UseWaitCursor = False
            btnGo.Enabled = True
        End Try

    End Sub

    Private Sub cmbServer_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbServer.SelectedIndexChanged
        Dim dirRoot As String = ""
        Dim myDir As String = ""


        Dim dt As DataTable = dsTable.Tables(0)
        'Dim query = (From row In dt.AsEnumerable() Where row.Field(Of String)("SERVER_NAME").Contains(cmbServer.Text) Select row)
        Dim query = (From row In dt.AsEnumerable() Where row.Field(Of String)("SERVER_NAME").Equals(cmbServer.Text) Select row)
        cmbSites.Items.Clear()
        For Each row In query
            cmbSites.Items.Add(row("SITE_NAME").ToString())
        Next
        cmbSites.SelectedIndex = 0

        If cmbServer.Text = "IMS" Then
            dirRoot = "\\ims\c$\builds\"
        ElseIf cmbServer.Text = "SDIX2012CLONE" Then
            dirRoot = "\\sdix2012clone\d$\A_Zeus_Copy\"
        ElseIf cmbServer.Text = "SDIX2012ZEUS2" Then
            dirRoot = "\\sdix2012zeus2\d$\PROD_Publish\"
        ElseIf cmbServer.Text = "SDIX2012" Then
            dirRoot = "\\sdix2012\d$\PROD_Publish\"
        End If
        Dim fl As New DirectoryInfo(dirRoot)
        myDir = (From f In fl.GetDirectories() Order By f.LastWriteTime Descending Select f).First.ToString

        txtSource.Text = dirRoot & myDir

    End Sub

    Private Sub txtSource_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles txtSource.MouseDoubleClick
        Process.Start(txtSource.Text)
    End Sub

    Private Sub btnOpenSite_Click(sender As Object, e As EventArgs) Handles btnOpenSite.Click
        Process.Start(cmbSites.Text)
    End Sub

    Private Sub cmbSites_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbSites.SelectedIndexChanged
        Dim data As DataTable
        Dim strSiteDesc As String = ""

        Dim dt As DataTable = dsTable.Tables(0)
        data = (From row In dt.AsEnumerable() Where row.Field(Of String)("SITE_NAME").Contains(cmbSites.Text) Select row).CopyToDataTable()
        strServerFolder = data.Rows(0).Item("WWWROOT").ToString

        strSiteDesc = data.Rows(0).Item("DESCR").ToString
        lblSiteDesc.Text = strSiteDesc
        lblSiteDesc.ForeColor = Color.Green

        'test reference
        'Select Case cmbSites.Text
        '    Case "http://sdiexchtest.sdi.com/"
        '        strServerFolder = "SDIExchTest"
        '    Case "http://ims.sdi.com:8080/SDICONNECT/"
        '        strServerFolder = "InsiteOnlineNew"
        '    Case "http://ims.sdi.com:8080/UNILOG/"
        '        strServerFolder = "8080Unilog"
        '    Case "http://ims.sdi.com:8073/"
        '        strServerFolder = "NewSdiExchIMS"
        '    Case "http://ims.sdi.com:8913/"
        '        strServerFolder = "Exchange11g"
        '    Case "http://ims.sdi.com:8080/CARTTEST/"
        '        strServerFolder = "TestShopCart"

        '    Case "http://zeustest.sdi.com/"
        '        strServerFolder = "TestSecureClone"
        '    Case "http://zeustest.sdi.com:8083/"
        '        strServerFolder = "TestExch8083_ZEUS"
        '    Case "http://zeustest.sdi.com:8086/"
        '        strServerFolder = "8086test"
        'End Select

        Select Case cmbServer.Text
            Case "SDIX2012"
                txtWebConfig.Text = System.Configuration.ConfigurationSettings.AppSettings("SDIXProdConfigFiles") & "\web.config"
            Case "SDIX2012ZEUS2"
                txtWebConfig.Text = System.Configuration.ConfigurationSettings.AppSettings("ZeusProdConfigFiles") & "\web.config"
            Case Else 'test sites
                txtWebConfig.Text = "\\" & cmbServer.Text & "\c$\copyforvr\backup\" & strServerFolder & "\web.config"
        End Select

        SearchForServerInWebConfig()

    End Sub

    Private Sub btnWebConfig_Click(sender As Object, e As EventArgs) Handles btnWebConfig.Click
        FolderBrowserDialog1.SelectedPath = "\\" & cmbServer.Text & "\c$\copyforvr\backup\" & strServerFolder
        'FolderBrowserDialog1.ShowDialog()

        If DialogResult.OK = FolderBrowserDialog1.ShowDialog Then
            txtWebConfig.Text = FolderBrowserDialog1.SelectedPath.ToString & "\web.config"
        End If

        SearchForServerInWebConfig()

    End Sub

    Private Sub SearchForServerInWebConfig()
        Dim strWebConfigDir As String = ""
        strWebConfigDir = txtWebConfig.Text.Substring(0, txtWebConfig.Text.LastIndexOf("\"))
        Try
            If Directory.Exists(strWebConfigDir) Then

                Dim myvalue As Integer
                For Each k As String In IO.File.ReadLines(txtWebConfig.Text)
                    If k.ToUpper.Contains("DATA SOURCE=") Then
                        myvalue = k.ToUpper.IndexOf("DATA SOURCE=")
                        lblWebConfigSrv.Text = k.Substring(myvalue + 12, 4)
                        lblWebConfigSrv.ForeColor = Color.Green
                        Exit For
                    End If
                Next
            Else
                lblWebConfigSrv.Text = "FILE NOT FOUND"
                lblWebConfigSrv.ForeColor = Color.Red
            End If


        Catch ex As Exception
            lblWebConfigSrv.Text = "SERVER NOT FOUND"
            lblWebConfigSrv.ForeColor = Color.Red

        End Try

    End Sub

    Private Sub btnTable_Click(sender As Object, e As EventArgs) Handles btnTable.Click
        TableEdit.Show()
    End Sub

    Private Sub txtWebConfig_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles txtWebConfig.MouseDoubleClick
        Dim strWebConfigDir As String = ""
        strWebConfigDir = txtWebConfig.Text.Substring(0, txtWebConfig.Text.LastIndexOf("\"))
        Try
            Process.Start(strWebConfigDir)
        Catch ex As Exception
        End Try
    End Sub

    'Private Sub btnChkWebConfig_Click(sender As Object, e As EventArgs) Handles btnChkWebConfig.Click
    '    Dim strWebConfigDir As String = ""
    '    strWebConfigDir = txtWebConfig.Text.Substring(0, txtWebConfig.Text.LastIndexOf("\"))
    '    If Directory.Exists(strWebConfigDir) Then
    '        SearchForServerInWebConfig()
    '    Else
    '        lblWebConfigSrv.Text = "FILE NOT FOUND"
    '        lblWebConfigSrv.ForeColor = Color.Red
    '    End If
    'End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click


        Dim tst As System.IO.StreamWriter
        tst = My.Computer.FileSystem.OpenTextFileWriter("readyforiisstart.txt", True)
        tst.WriteLine("Done.")
        tst.Close()

        Do
            If My.Computer.FileSystem.FileExists("readyforiisstart.txt") Then
                My.Computer.FileSystem.DeleteFile("readyforiisstart.txt")
                Exit Do
            End If
            Sleep(1000)
        Loop


        Dim result As Integer = MessageBox.Show("Test IIS Reset on local machine?  Are you sure?!?!?!!", "Test IIS Reset", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation)
        If result = DialogResult.Cancel Then
            Exit Sub
        End If
        'for test only

        lblMessage.AppendText("STOPPING IIS" & vbCrLf)
        Me.Refresh()
        Application.DoEvents()


        'Dim stopinfo As New ProcessStartInfo("iisreset.exe", "/stop")
        Dim stopinfo As New ProcessStartInfo("test.bat")

        'System.Diagnostics.Process.Start(stopinfo)
        'stopinfo.FileName = "/c c:\windows\system32\iisreset.exe /help"
        stopinfo.WindowStyle = ProcessWindowStyle.Normal
        'stopinfo.WindowStyle = ProcessWindowStyle.Hidden
        stopinfo.Verb = "runas"
        'Process.Start(stopinfo)
        Dim p1 As New Process
        p1.StartInfo = stopinfo
        p1.Start()
        p1.WaitForExit()
        p1.Close()

        MsgBox("Stopped IIS")

        lblMessage.AppendText("RESTARTING IIS" & vbCrLf)
        Me.Refresh()
        Application.DoEvents()

        'Dim startinfo As New ProcessStartInfo("iisreset.exe", "/start")
        Dim startinfo As New ProcessStartInfo("test.bat")

        'startinfo.FileName = "iisreset.exe /start"
        startinfo.WindowStyle = ProcessWindowStyle.Normal
        'stopinfo.WindowStyle = ProcessWindowStyle.Hidden
        startinfo.Verb = "runas"
        Dim p2 As New Process
        p2.StartInfo = startinfo
        p2.Start()
        p2.WaitForExit()
        p2.Close()

        MsgBox("Re-Started IIS")

        'Process.Start("\\" & cmbServer.Text & "\c$\inetpub\wwwroot\" & strServerFolder)

        'Exit Sub


        'copyFrom = txtSource.Text
        'copyTo = "C:\temp\siteuploadutility_temp\"

        'Dim cnt As Integer
        'cnt = Directory.GetFiles(copyFrom, "*.*", SearchOption.AllDirectories).Count '100
        'ProgressBar1.Maximum = cnt
        'ProgressBar1.Step = 1
        'ProgressBar1.Value = 0
        'lblProgress.Visible = True
        'BackgroundWorker1.RunWorkerAsync()

        'Timer1.Enabled = True

        'While doneReplace = False
        '    Me.Refresh()
        '    Application.DoEvents()
        '    System.Threading.Thread.Sleep(5000)
        'End While

        'MsgBox("All done")

    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        Try

            'For i = 0 To 100
            '    System.Threading.Thread.Sleep(100)
            '    BackgroundWorker1.ReportProgress(i)
            'Next i

            'Dim delFilesCount = 0
            'For Each di In Directory.GetDirectories(strTargetDir)
            '    'Directory.Delete(di, True)
            '    delFilesCount += 1
            '    BackgroundWorker1.ReportProgress(delFilesCount)
            '    System.Threading.Thread.Sleep(50)
            'Next
            'For Each delFile In Directory.GetFiles(strTargetDir, "*.*")
            '    'File.Delete(delFile)
            '    delFilesCount += 1
            '    BackgroundWorker1.ReportProgress(delFilesCount)
            '    System.Threading.Thread.Sleep(50)
            'Next

            Timer1.Enabled = True

            My.Computer.FileSystem.CopyDirectory(copyFrom, copyTo, True)

        Catch ex As Exception
        End Try

    End Sub

    Private Sub BackgroundWorker1_ProgressChanged(sender As Object, e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
        ProgressBar1.Value = e.ProgressPercentage
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim iResponse As Integer
        Dim cnt = Directory.GetFiles(copyTo, "*.*", SearchOption.AllDirectories).Count

        lblProgress.Text = "File " & cnt & " of " & ProgressBar1.Maximum & " copied."
        ProgressBar1.Value = cnt
        If cnt = ProgressBar1.Maximum Then
            Timer1.Enabled = False
            'doneReplace = True

            Try
                'db name, description, deploy date
                'lblMessage.Text += "Updating SDIX_DEPLOY_SITES table" & vbCrLf
                lblMessage.AppendText("Updating SDIX_DEPLOY_SITES table" & vbCrLf)
                Me.Refresh()
                Application.DoEvents()
                strSQLString = "UPDATE SDIX_DEPLOY_SITES SET "
                strSQLString += " DB_NAME = '" & lblWebConfigSrv.Text & "',"
                strSQLString += " DEPLOY_DATE = SYSDATE "
                If txtDescription.Text.Trim <> "" Then
                    txtDescription.Text = Replace(txtDescription.Text, "'", "''")
                    strSQLString += ",DESCR = '" & txtDescription.Text & "'"
                End If
                strSQLString += " WHERE SITE_NAME = '" & cmbSites.Text & "'"
                iResponse = ORDBData.ExecNonQuery(strSQLString)
            Catch ex As Exception
                lblMessage.Text = ex.ToString
                lblMessage.ForeColor = Color.Red
                Exit Sub
            End Try
            '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

            lblMessage.ForeColor = Color.Green
            'lblMessage.Text += "Process complete!"
            lblMessage.AppendText("Process complete!")

            MsgBox("Process complete!")


        End If
    End Sub

End Class
