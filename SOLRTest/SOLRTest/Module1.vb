Imports System
Imports System.Net
Imports System.Text
Imports System.IO
'Imports System.Web.Services
'Imports Insiteonline.WebPartnerFunctions.WebPSharedFunc
Imports System.Data
Imports System.Web
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Collections.Generic
Imports System.Data.OleDb
Imports System.Net.Security
Imports System.Xml


Module Module1

    Sub Main()

        Dim jsonResponse As String = ""
        Dim urlAddress As String = "https://zeussdixsolr:JCve!6is@zeusclwk4.isacs.com:8985/solr/sdix/select?q=lamp&fq=subset_id:256&sort=score+desc&fl=*,score&wt=json&indent=true&defType=dismax&qf=id%20product_name%20short_desc%20subset_id%20unspsc%20manufacturer_part_number%20manufacturer_part_number_s%20customer_part_number_woprefix%20customer_part_number_woprefix_numeric%20vendor%20brand_name%20product_catagory%20product_sub_category%20long_desc%20client_desc%20extended_desc%20uom%20etag%20product_attr&pf=desc_ns%5E50 short_desc%5E10 long_desc%5E30 client_desc%5E20 &boost=if(termfreq(catalog_ind,1),1.5,1)&lowercaseOperators=true&ps=5&hl=true&hl.fl=product_category%20short_desc%20long_desc%20client_desc%20extended_desc&hl.simple.pre=%3Cem%3E&hl.simple.post=%3C%2Fem%3E&hl.usePhraseHighlighter=true&hl.highlightMultiTerm=true&facet=true&facet.field=manufacturer&facet.mincount=1&rows=0&spellcheck=true&spellcheck.count=4&shards=shard1,shard2,shard3"
        Dim userName As String = "cimm2bsdiclient"
        Dim password As String = "@c!mm@bsdicl!ent"
        Dim searchValue As String = "lamp"
        Dim ErrorMsg As String = ""


        Console.WriteLine("Start SOLR Test")

        jsonResponse = CommonGetResults(urlAddress, userName, password, searchValue, ErrorMsg, True)

        If UCase(Trim(jsonResponse)) = "FAILED" Then
            'down

            Console.WriteLine("SOLR is Down. Error: " & ErrorMsg)

        Else
            'up

            Console.WriteLine("SOLR is Up and running")

        End If

        Stop

        Console.WriteLine("End SOLR Test")

    End Sub

    Private Function CommonGetResults(urlAddress As String, userName As String, password As String, _
                    searchValue As String, Optional ByRef strErr As String = "", Optional ByVal bIsSolrSearch As Boolean = False) As String

        ' Dim currentApp As HttpApplication = HttpContext.Current.ApplicationInstance

        Dim response As String = ""  '  String.Empty
        Dim basicAuthBase641 As String = ""
        Try
            Dim req As WebRequest = WebRequest.Create(urlAddress)
            req.Method = "GET"
            If Not bIsSolrSearch Then
                req.Credentials = New NetworkCredential(userName, password)
                req.UseDefaultCredentials = False
                basicAuthBase641 = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(String.Format("{0}:{1}", userName, password)))
                req.Headers.Add("Authorization", String.Format("Basic {0}", basicAuthBase641))
            Else
                req.UseDefaultCredentials = True
            End If

            Dim resp As HttpWebResponse = TryCast(req.GetResponse(), HttpWebResponse)
            If resp.StatusCode = HttpStatusCode.OK Then
                Using respStream As Stream = resp.GetResponseStream()
                    Dim reader As New StreamReader(respStream, Encoding.UTF8)
                    response = reader.ReadToEnd()
                End Using
            Else
                response = "Failed"
            End If
        Catch ex As Exception
            Dim errMsg As String = Convert.ToString(ex.Message) & "-" & Convert.ToString(ex.InnerException)
            If errMsg.Length > 49 Then
                errMsg = errMsg.Substring(0, 49)
            End If
            'Try

            '    strErr = "Error in UnilogSearch.vb, CommonGetResults " & vbCrLf
            '    strErr += " BU: " & currentApp.Session("BUSUNIT") & "  User: " & currentApp.Session("USERID") & " Search String: " & searchValue.Trim() & " Catalog Id: " & CType(currentApp.Session("CplusDbSQL"), String) & vbCrLf _
            '        & " Error: " & Trim(ex.Message)
            '    SendSDiExchErrorMail(strErr, "Error in UnilogSearch.vb, CommonGetResults ")
            '    SearchQueryAudit("Error Search Result", Convert.ToString(currentApp.Session("USERID")), Convert.ToString(currentApp.Session("BUSUNIT")), searchValue, errMsg, "-1")

            'Catch exJsnErr As Exception

            'End Try
            response = "Failed"
        End Try

        Return response

    End Function

End Module
