Imports System
Imports System.Data
Imports System.IO
Imports System.Data.OleDb
Imports System.Web
Imports System.Xml
Imports System.Text
Imports System.Net
Imports System.Security.Cryptography.X509Certificates
Imports System.Net.Mail
Imports SDI.ApplicationLogger
Imports SDI.UNCC.WorkOrderAdapter

Module Module1

    Private m_logger As appLogger = Nothing
    Private myLoggr1 As appLogger = Nothing
    'Private m_POConfirm_Logger As appLogger = Nothing
    Private m_POConfirm_LoggerN1 As appLogger = Nothing

    Dim rootDir As String = "C:\LMIn"
    Dim logpath As String = "C:\LMIn\LOGS\KLATencorPOInToINTF" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sErrLogPath As String = "C:\LMIn\LOGS\MyErredSQLs" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sPOConfirmPath As String = "C:\LMIn\SFTP\POConfrm\POConfirm" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim sPoConfirmText As String = ""
    Dim connectOR As New OleDbConnection("Provider=OraOLEDB.Oracle.1;Password=sd1exchange;User ID=sdiexchange;Data Source=STAR")
    Dim connectSQL As New SqlClient.SqlConnection("server=cplus_prod;uid=einternet;pwd=einternet;initial catalog='contentplus'")
    Dim strOverride As String
    Dim bolWarning As Boolean = False

    Private m_arrXMLErrFiles As String
    Private m_arrErrorsList As String

    Sub Main()

    End Sub

End Module
