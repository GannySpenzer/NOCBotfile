Imports System.Web.Services
Imports System
Imports System.Data.OleDb
Imports System.IO
Imports System.Web.Mail
Imports System.Web.UI
Imports System.Text
Imports System.Xml.Serialization
Imports System.Runtime.Serialization
 
<System.Web.Services.WebService(Namespace:="http://www.sdiexchange.com")> _
Public Class SDI_loadbalance_IO
    Inherits System.Web.Services.WebService
    'Protected WithEvents dirContent As System.Web.UI.WebControls.Label
    'Protected WithEvents message As System.Web.UI.WebControls.Label
    'Protected WithEvents dirContent As System.Web.UI.WebControls.Label
    'Protected WithEvents message As System.Web.UI.WebControls.Label
    'Protected WithEvents uploadedFile As System.Web.UI.HtmlControls.HtmlInputFile
    'Protected WithEvents upload As System.Web.UI.HtmlControls.HtmlInputButton



#Region " Web Services Designer Generated Code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Web Services Designer.
        InitializeComponent()

        'Add your own initialization code after the InitializeComponent() call

    End Sub

    'Required by the Web Services Designer
    Private components As System.ComponentModel.IContainer
    Dim directorySeparatorChar As Char = Path.DirectorySeparatorChar

    'NOTE: The following procedure is required by the Web Services Designer
    'It can be modified using the Web Services Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        components = New System.ComponentModel.Container
    End Sub

    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        'CODEGEN: This procedure is required by the Web Services Designer
        'Do not modify it using the code editor.
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

#End Region

    ' WEB SERVICE EXAMPLE
    ' The HelloWorld() example service returns the string Hello World.
    ' To build, uncomment the following lines then save and build the project.
    ' To test this web service, ensure that the .asmx file is the start page
    ' and press F5.
    '
    '<WebMethod()> _
    'Public Function HelloWorld() As String
    '   Return "Hello World"
    'End Function
    <WebMethod()> Public Function Stat_Change_Email_Copy(ByVal filepath As String, ByVal StrMessage As String) As String
        Dim objStreamWriterLogs As StreamWriter
        Dim objStreamWriterFiles As StreamWriter

        objStreamWriterLogs = File.CreateText(filepath)

        objStreamWriterLogs.WriteLine(StrMessage)
        objStreamWriterLogs.Close()
        Return ""
    End Function
    <WebMethod()> Public Function Stat_Change_Email_Send(ByVal filepath As String) As String


        ' the web service determines where the overflow email resides
        ' emails > 3999 characters get copied to a text file.
        ' Before the load balancer, it went to the server where ISOL resided.
        ' Now we'll have three servers, so we will let IIS determine where the text files reside.
        ' The web service is named SDI_load_balance_IO.
        Dim objStreamWriter As StreamWriter


        Dim reader As TextReader
        Try
            If File.Exists(filepath) Then
                reader = File.OpenText(filepath)
            Else
                objStreamWriter.WriteLine("  " & filepath & " does not exist")
                reader.Close()
                Return ""
            End If
        Catch ex As Exception
            Return ""
        End Try




        Dim readerline As String
        Dim I As Integer
        Try
            While reader.Peek <> -1
                readerline = readerline & reader.ReadLine()
            End While

            reader.Close()
            Return readerline
        Catch ex As Exception
            objStreamWriter.WriteLine("  " & filepath & " - " & ex.Message)
        End Try
        reader.Close()
        Return ""

    End Function
    <WebMethod()> Public Function Stat_Change_Email_check_dir_exist(ByVal rootdir As String) As String
        Dim objStreamWriterLogs As StreamWriter
        Dim objStreamWriterFiles As StreamWriter
        Dim objStreamWriter As StreamWriter


        Try
            If Dir(rootdir, FileAttribute.Directory) = "" Then
                MkDir(rootdir)
            End If
            If Dir(rootdir & "\LOGS", FileAttribute.Directory) = "" Then
                MkDir(rootdir & "\LOGS")
            End If
            If Dir(rootdir & "\FILES", FileAttribute.Directory) = "" Then
                MkDir(rootdir & "\FILES")
            End If
        Catch ex As Exception
            objStreamWriter.WriteLine("  " & rootdir & " - " & ex.Message)
            Return "Error " & ex.Message
        End Try
        Return "     "

    End Function
    '<WebMethod()> Public Function Upload_Attachment(ByVal currentDir As String, ByVal directorySeparatorChar As String, ByVal filename As String, ByVal contenttype As String, ByVal contentlength As String)
    '<WebMethod()> Public Function Upload_Attachment(ByVal file_path As String, ByVal file_name As String)
    <WebMethod()> Public Function Upload_Attachment(ByVal file_path As HttpPostedFile)
        Dim input_file As File
        ''ByVal uploadedFile As System.Web.UI.HtmlControls.HtmlInputFile, ByVal currentDir As String
        Dim root As String = "C:\Ticket_attachments\"
        Dim out_filepath As String = "C:\Ticket_attachments\"

        'file_path = (HttpUtility.HtmlDecode(file_path.Trim))
        Dim out_filepath1 As String = (HttpUtility.HtmlDecode(out_filepath))
        'System.Web.HttpPostedFile.InputStream
        ' Dim io_string As System.Web.HttpPostedFile.InputStream(file_path)
        'inPUTSTREAM

        'Dim file_name1 As String = (HttpUtility.HtmlDecode(file_name))
        'Dim out_path As String = out_filepath1 + file_name1
        'input_file.Copy(ROOT  & "\" & UPLOADEDFILE, FILEPATH & UPLOADEDFILE
        'httputility.htmldecode(file_path)
        'FileCopy(HttpUtility.HtmlDecode(file_path), HttpUtility.HtmlDecode(out_filepath & file_name))
        ' File.Copy(file_path, out_filepath & out_path)

        ' File.Copy(file_path, out_filepath)

        ' currentDir = Request.Params("dir")
        'If currentDir Is Nothing Then
        '    currentDir = root
        'End If


        Dim objStreamWriterLogs As StreamWriter
        Dim objStreamWriterFiles As StreamWriter
        Dim objStreamWriter As StreamWriter


        'objStreamWriterLogs = File.CreateText(filepath)
        'objStreamWriterLogs.WriteLine(StrMessage)
        'objStreamWriterLogs.Close()







        'objStreamWriterLogs.Close()
        'If Not (uploadedFile.PostedFile Is Nothing) Then
        '    Try
        '        Dim postedFile = uploadedFile.PostedFile
        '        Dim filename As String = Path.GetFileName(postedFile.FileName)
        '        Dim contentType As String = postedFile.ContentType
        '        Dim contentLength As Integer = postedFile.ContentLength

        '        postedFile.SaveAs(currentDir & _
        '          directorySeparatorChar.ToString() & filename)
        '    Catch ex As Exception
        '        message.Text = "Failed uploading file"
        '    End Try
        'End If
    End Function
    'could use the statemail check but we'll use this so not to confuse
    <WebMethod()> Public Function does_attachment_dir_exist(ByVal rootdir As String) As String
        Dim objStreamWriterLogs As StreamWriter
        Dim objStreamWriterFiles As StreamWriter
        Dim objStreamWriter As StreamWriter
        Try
            If Dir(rootdir, FileAttribute.Directory) = "" Then
                MkDir(rootdir)
            End If
            If Dir(rootdir & "\LOGS", FileAttribute.Directory) = "" Then
                MkDir(rootdir & "\LOGS")
            End If
            If Dir(rootdir & "\FILES", FileAttribute.Directory) = "" Then
                MkDir(rootdir & "\FILES")
            End If
        Catch ex As Exception
            objStreamWriter.WriteLine("  " & rootdir & " - " & ex.Message)
            Return "Error " & ex.Message
        End Try
        Return "     "
    End Function
End Class


