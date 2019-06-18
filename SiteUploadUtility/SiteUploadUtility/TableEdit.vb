Imports System.Data
Imports System.Data.OleDb

Public Class TableEdit

    Dim strsqlString As String = ""
    Dim dsTable As DataSet

    Private con As OleDbConnection = New OleDbConnection
    Private Dadapter As OleDbDataAdapter
    Private DSet As DataSet
    Private DSet2 As DataSet
    Private ConCMD As OleDb.OleDbCommand

    Private Sub TableEdit_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        'strsqlString = "SELECT * FROM SDIX_DEPLOY_SITES"
        'dsTable = ORDBData.GetAdapter(strsqlString)

        'DataGridView1.DataSource = dsTable.Tables(0)

        Dim myConString As String = ORDBData.DbUrl  '"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=e:\Database31.accdb"
        con.ConnectionString = myConString
        con.Open()
        ConCMD = New OleDbCommand("SELECT SITE_NAME, SERVER_NAME, BRANCH, WWWROOT, DB_NAME, DESCR, DEPLOY_DATE, ADDED_BY, ADDED_DATE FROM SDIX_DEPLOY_SITES", con)
        'Dadapter = New OleDbDataAdapter("select * from SDIX_DEPLOY_SITES", con)
        Dadapter = New OleDbDataAdapter(ConCMD)
        DSet = New DataSet
        Dadapter.Fill(DSet, "SDIX_DEPLOY_SITES")
        DataGridView1.DataSource = DSet.Tables("SDIX_DEPLOY_SITES")
        con.Close()

        lblDatabase.Text = myConString.Substring(Len(myConString) - 4)

    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Using con = New OleDbConnection(ORDBData.DbUrl)
            'DSet = DataGridView1.DataSource
            Dadapter.UpdateCommand = ConCMD
            Dim cb As OleDbCommandBuilder = New OleDbCommandBuilder(Dadapter)
            'Dadapter.Update(DSet, "SDIX_DEPLOY_SITES")
            Dadapter.Update(DSet.Tables("SDIX_DEPLOY_SITES"))

            DSet.AcceptChanges()
        End Using


    End Sub
End Class