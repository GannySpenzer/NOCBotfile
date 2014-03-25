Imports System.Data.SqlClient
Imports System.Data.OleDb
Imports update_classavaiableProducts.ORDBAccess
Imports update_classavaiableProducts.SQLDBAccess
Imports System.Xml
Imports System.IO
 Imports System.Text


Module Module1
    ' to run this these preliminary steps have to be done... 
    ' load the excel spread sheet into Junction_load MSAccess File
    ' load the access BS into the SQlsever MaintainedShippedItemTable755784 in cplus_prod/contentplus/MaintainedShippedItemTable755784
    ' then run this f'en program
    ' have a beer and call me in the morning
    ' the ladies of the night should be gone by then!!!!!
    ' You then need to run the publish
    ' Why this needs to be done is because when the item is changed in Eplus it blanks out the 
    ' field customeritemid... a field that ePlus knows nothing about.
    ' We need to reload that field into classavailableproducts
    Dim strbu As String = " "
    Dim strWhatDB = "prod"
    Dim objStreamWriter As StreamWriter
    Dim rootDir As String = "C:\ProdViewSites"
    Dim logpath As String = "C:\ProdViewSites\LOGS\classavailfix" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"

    Dim connectsql As New SqlConnection("server=192.168.253.52;uid=sa;pwd=coca-cola;initial catalog=contentplus")

    Dim connectORPROD As New OleDbConnection("Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=prod")
    Dim connectORPLGR As New OleDbConnection("Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=plgr")
    Dim strproductviewid As String = " "
    'Dim strproductviewid As String = "272" 'I0469'
    'Dim strproductviewid As String = "219" 'I0712'
    'Dim strproductviewid As String = "52" 'I0230'
    ' Dim strproductviewid As String = "134" 'I0260'
    'Dim strproductviewid As String = "170" 'I0409'
    ' Dim strproductviewid As String = "272" 'I0457'
    'Dim strproductviewid As String = "275" 'I0456'
    'Dim strproductviewid As String = "12" 'penn 
    Dim ctr As Integer = 0
    Dim connector As OleDbConnection
    'Dim connectsql As SqlConnection
    Sub Main()
        Console.WriteLine("The classavalableproducts update  = " & strWhatDB)
        Console.WriteLine("")
        If Dir(rootDir, FileAttribute.Directory) = "" Then
            MkDir(rootDir)
        End If
        If Dir(rootDir & "\LOGS", FileAttribute.Directory) = "" Then
            MkDir(rootDir & "\LOGS")
        End If
        objStreamWriter = File.CreateText(logpath)
        objStreamWriter.WriteLine("  Send emails out " & Now())

        build_BU_prodview()
        Dim dsCustom5 As DataSet
        'build the items from classavailableproducts that have a 0 in the customeritemid field for PENN
        Dim ds As DataSet = createprodviewitems(strproductviewid)

        If ds.Tables(0).Rows.Count = 0 Then
            Console.WriteLine("No records")
        Else
            Console.WriteLine("   Begin update of items")
            objStreamWriter.WriteLine("  End of classavailfix Number of items =" & ctr & " " & Now())
            'build the dataset from ps_isa_cp_junction table

            buildfromCpjunction(ds)
            Console.WriteLine("Number of items = " & ctr)

        End If
        objStreamWriter.WriteLine("  End of classavailfix Number of items =" & ctr & " " & Now())

        objStreamWriter.Flush()
        objStreamWriter.Close()
    End Sub
    Sub buildfromCpjunction(ByVal ds As DataSet)

        Dim I As Integer
        Dim X As Integer
        Dim DS1 As DataSet
        Dim rowsaffected As Integer
        Dim dsProdview As DataSet
        Dim intExist As Integer
        Dim intnewitemid As Integer
        Dim intolditemid As Integer

        Dim strSQLstring As String
        Try
            For I = 0 To ds.Tables(0).Rows.Count - 1





                strSQLstring = "select inv_item_id, isa_site_id from  PS_ISA_CP_JUNCTION" & _
                    " where ISA_CP_PROD_ID = '" & ds.Tables(0).Rows(I).Item(0) & "'" & _
                 " and ISA_site_id = '" & strbu & "'"
                Try

                    DS1 = GetAdapter(strSQLstring, connectORPROD)
                Catch OLEdbExp As SqlException
                    MsgBox(OLEdbExp.ToString, MsgBoxStyle.Critical)
                    Exit For
                End Try
                If DS1.Tables(0).Rows.Count > 0 Then



                    Dim strcustitem As String = DS1.Tables(0).Rows(0).Item(0)
                    Dim strsiteid As String = DS1.Tables(0).Rows(0).Item(1)
                    'custodial keeps the leading three site values
                    If strsiteid <> "231" Then
                        strcustitem = strcustitem.Substring(3)
                    End If
                    'Do the final cleanup such as closing the Database connection
                    '
                    strSQLstring = "UPDATE  classavailableproducts" & _
                          " SET customeritemid =  '" & strcustitem & "'" & _
                          " Where itemid = '" & ds.Tables(0).Rows(I).Item(0) & "'" & _
                          " And productviewid = '" & strproductviewid & "'"
                    ' " And productviewid = '11'"
                    ' " And productviewid = '12'"

                    Try
                        rowsaffected = ExecNonQuerySQL(strSQLstring, connectsql)


                    Catch SQLExp As SqlException
                        MsgBox(SQLExp.ToString, MsgBoxStyle.Critical)
                    Finally
                        'Do the final cleanup such as closing the Database connection
                    End Try
                End If

                ctr = ctr + 1
            Next
            connector.Close()
            connectsql.Close()
        Catch ex As Exception

        End Try
    End Sub
    Function createprodviewitems(ByVal prodviewid As String) As DataSet

        Dim strSQLString As String = "SELECT itemid" & _
                   " FROM classavailableproducts" & _
                   " WHERE productviewid = '" & prodviewid & "'" & _
        " AND customeritemid is null or customeritemid= ' ' "

        createprodviewitems = GetSQLAdapter(strSQLString, connectsql)

    End Function
    Function build_BU_prodview()
        ' get XML file of sites that require email
        Dim strXMLDir As String = rootDir & "\ProdViewSites.xml"
        Dim xmldata As New XmlDocument
        Dim sr As System.IO.StreamReader
        Dim XMLContent As String
        Dim jobNode As XmlNode
        sr = New System.IO.StreamReader(strXMLDir)
        XMLContent = sr.ReadToEnd()
        sr.Close()
        xmldata.LoadXml(XMLContent)
        Dim jj As XmlNode = xmldata.ChildNodes(2)
        jobNode = xmldata.ChildNodes(1)
        Dim dsRows As New DataSet
        dsRows.ReadXml(New XmlNodeReader(jobNode))
        Dim I As Integer
        Dim bolErrorSomeWhere As Boolean

        strbu = dsRows.Tables(0).Rows(I).Item("SITEBU")
        strproductviewid = dsRows.Tables(0).Rows(I).Item("SITEPRODVIEW")









    End Function
End Module
