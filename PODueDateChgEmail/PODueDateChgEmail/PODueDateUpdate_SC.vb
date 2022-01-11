Imports System.Configuration
Imports System.Data.OleDb
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports Newtonsoft.Json

Public Class PODueDateUpdate_SC

    Dim objWalmartSC As StreamWriter
    Dim rootDir As String = "C:\DueDateUpdate"
    Dim WalmartSC As String = "C:\DueDateUpdate\LOGS\WalmartSC" & Now.Year & Now.Month & Now.Day & Now.GetHashCode & ".txt"
    Dim connectOR As New OleDbConnection(Convert.ToString(ConfigurationManager.AppSettings("OLEDBconString")))

    Public Function PODueDateUpdate(ByVal MultiplePO As String)
        Try
            objWalmartSC = File.CreateText(WalmartSC)
            objWalmartSC.WriteLine("Started Walmart Service Channel " & Now())
            objWalmartSC.WriteLine("PO_ID List-" + MultiplePO + "")
            Dim strSQLstrings As String = "select * from ps_isa_poduedtmon where PO_ID in (" & MultiplePO & ") and BUSINESS_UNIT= 'WAL00' and TRUNC(ADD_DTTM) = TRUNC(SYSDATE)
                                            order by ADD_DTTM desc" & vbCrLf

            Dim dsPOset As DataSet = ORDBAccess.GetAdapter(strSQLstrings, connectOR)
            If dsPOset.Tables.Count > 0 Then
                If dsPOset.Tables(0).Rows.Count > 0 Then
                    For Each dr As DataRow In dsPOset.Tables(0).Rows
                        Dim PO_ID As String = dr.Item("PO_ID")
                        Dim LINE_NBR As String = dr.Item("LINE_NBR")
                        Dim SCHED_NBR As String = dr.Item("SCHED_NBR")
                        Dim DUE_DT As String = dr.Item("DUE_DT")
                        objWalmartSC.WriteLine("PO_ID -" + PO_ID + ",LINE_NBR -" + LINE_NBR + ", SCHED_NBR -" + SCHED_NBR)
                        strSQLstrings = "select LN.*,LN.ISA_INTFC_LN, LD.PO_ID ,LN.ISA_WORK_ORDER_NO, US.THIRDPARTY_COMP_ID from PS_PO_LINE_DISTRIB LD, ps_isa_ord_intf_LN LN, SDIX_USERS_TBL US" & vbCrLf &
            " where LD.Req_id = LN.order_no AND LD.REQ_LINE_NBR = LN.ISA_INTFC_LN And US.ISA_EMPLOYEE_ID= LN.OPRID_ENTERED_BY" & vbCrLf &
            " And LN.BUSINESS_UNIT_OM = 'I0W01' and LD.PO_ID= '" & PO_ID & "' and LD.LINE_NBR = '" & LINE_NBR & "' and LD.SCHED_NBR ='" & SCHED_NBR & "'" & vbCrLf

                        Dim dSet As DataSet = ORDBAccess.GetAdapter(strSQLstrings, connectOR)

                        If dSet.Tables.Count > 0 Then
                            If dSet.Tables(0).Rows.Count > 0 Then
                                Dim OrderNo As String = String.Empty
                                Dim Work_Order As String = String.Empty
                                Dim ThirdParty_ID As String = String.Empty
                                Dim Desc As String = String.Empty
                                Try
                                    OrderNo = dSet.Tables(0).Rows(0).Item("order_no")
                                    Work_Order = dSet.Tables(0).Rows(0).Item("ISA_WORK_ORDER_NO")
                                    Desc = dSet.Tables(0).Rows(0).Item("DESCR254")
                                    ThirdParty_ID = dSet.Tables(0).Rows(0).Item("THIRDPARTY_COMP_ID")
                                Catch ex As Exception
                                    ThirdParty_ID = "0"
                                End Try
                                objWalmartSC.WriteLine("Order No -" + OrderNo + ", Work Order -" + Work_Order + ", Third Party ID -" + ThirdParty_ID)
                                If OrderNo.ToUpper.Substring(0, 1) = "W" Then
                                    If Not String.IsNullOrEmpty(Work_Order) Then
                                        Dim apiResponse As String = GetWorkOrderParts(Work_Order, ThirdParty_ID)
                                        If Not String.IsNullOrEmpty(apiResponse) Then
                                            Dim objWorkOrder As List(Of WorkOrderParts) = JsonConvert.DeserializeObject(Of List(Of WorkOrderParts))(apiResponse)
                                            Dim deletearrayOfID As New List(Of Int32)
                                            Dim deletearrayOfDesc As New List(Of String)
                                            For Each objWorkOrders As WorkOrderParts In objWorkOrder
                                                If objWorkOrders.Description.ToLower().Contains(Desc.Trim().ToLower()) And objWorkOrders.SupplierPartId.Contains(OrderNo) Then
                                                    deletearrayOfID.Add(objWorkOrders.id)
                                                    deletearrayOfDesc.Add(objWorkOrders.Description)
                                                    Dim deleteResponse As Boolean = DeleteWorkOrders(Work_Order, deletearrayOfID.ToArray(), ThirdParty_ID)
                                                    If deleteResponse = True Then
                                                        Dim dsCart As New DataTable
                                                        Dim drs As DataRow
                                                        dsCart.Columns.Add("ItemDescription")
                                                        dsCart.Columns.Add("Quantity")
                                                        dsCart.Columns.Add("Price")
                                                        dsCart.Columns.Add("UDueDate")
                                                        dsCart.Columns.Add("OrderNo")
                                                        drs = dsCart.NewRow()
                                                        drs.Item(0) = Desc
                                                        drs.Item(1) = objWorkOrders.Quantity
                                                        drs.Item(2) = objWorkOrders.Price
                                                        drs.Item(3) = DUE_DT
                                                        drs.Item(4) = objWorkOrders.SupplierPartId
                                                        dsCart.Rows.Add(drs)
                                                        If dsCart.Rows.Count > 0 Then
                                                            Dim insertResponse As Boolean = InsertWorkOrder(Work_Order, dsCart, ThirdParty_ID)
                                                            'If insertResponse = True Then
                                                            '    Dim rowsaffected As Integer
                                                            '    connectOR.Open()
                                                            '    Dim strSQLstring As String = "UPDATE ps_isa_poduedtmon" & vbCrLf &
                                                            '                   " SET NOTIFY_DTTM = TO_DATE('" & Now() & "', 'MM/DD/YYYY HH:MI:SS AM')" & vbCrLf &
                                                            '                   " WHERE PO_ID = '" & PO_ID & "'" & vbCrLf &
                                                            '                   " AND LINE_NBR = '" & LINE_NBR & "'" & vbCrLf &
                                                            '                   " AND SCHED_NBR = '" & SCHED_NBR & "'"

                                                            '    Dim Command1 As OleDbCommand
                                                            '    Command1 = New OleDbCommand(strSQLstring, connectOR)
                                                            '    Try
                                                            '        rowsaffected = Command1.ExecuteNonQuery
                                                            '        If rowsaffected = 1 Then
                                                            '            objWalmartSC.WriteLine("PO_ID -" + PO_ID + ", Updated Successfully")
                                                            '        Else
                                                            '            objWalmartSC.WriteLine("PO_ID -" + PO_ID + ", Updated Failed")
                                                            '        End If
                                                            '    Catch ex As Exception
                                                            '        objWalmartSC.WriteLine("PO_ID -" + PO_ID + ", Updated Failed")
                                                            '    End Try
                                                            '    Command1.Dispose()
                                                            '    Try
                                                            '        connectOR.Close()
                                                            '    Catch ex As Exception

                                                            '    End Try
                                                            'End If
                                                        End If
                                                    End If
                                                    objWalmartSC.WriteLine("////////////////////////////////////////////////////////////////////////////////////////////")
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    End If
                                End If
                            Else
                                objWalmartSC.WriteLine("////////////////////////////////////////////////////////////////////////////////////////////")
                            End If
                        Else
                            objWalmartSC.WriteLine("////////////////////////////////////////////////////////////////////////////////////////////")
                        End If
                        objWalmartSC.WriteLine("////////////////////////////////////////////////////////////////////////////////////////////")
                    Next
                End If
            End If
            objWalmartSC.WriteLine("Ends " & Now())
            objWalmartSC.Flush()
            objWalmartSC.Close()
        Catch ex As Exception

        End Try

    End Function

    Public Function GetWorkOrderParts(ByVal workOrder As String, Session_THIRDPARTY_COMP_ID As String) As String
        Try
            If Not String.IsNullOrEmpty(workOrder) And Not String.IsNullOrWhiteSpace(workOrder) Then
                Dim APIresponse = String.Empty
                If Session_THIRDPARTY_COMP_ID = ConfigurationManager.AppSettings("CBRECompanyID").ToString() Then
                    APIresponse = AuthenticateService("CBRE")
                Else
                    APIresponse = AuthenticateService("Walmart")
                End If
                If (APIresponse <> "Server Error" And APIresponse <> "Internet Error" And APIresponse <> "Error") Then
                    If (Not APIresponse.Contains("error_description")) Then
                        Dim objValidateUserResponseBO As ValidateUserResponseBO = JsonConvert.DeserializeObject(Of ValidateUserResponseBO)(APIresponse)
                        Dim apiURL = ConfigurationManager.AppSettings("ServiceChannelBaseAddress") + "/workorders/" + workOrder + "/parts"
                        Dim httpClient As HttpClient = New HttpClient()
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token)
                        Dim response = httpClient.GetAsync(apiURL).Result
                        If response.IsSuccessStatusCode Then
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result
                            If workorderAPIResponse <> "[]" And Not String.IsNullOrEmpty(workorderAPIResponse) And Not String.IsNullOrWhiteSpace(workorderAPIResponse) Then
                                objWalmartSC.WriteLine("Method: GetWorkOrderParts - Success")
                                Return workorderAPIResponse
                            Else
                                objWalmartSC.WriteLine("Method: GetWorkOrderParts - Failed")
                                Return String.Empty
                            End If
                        Else
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result
                            objWalmartSC.WriteLine("Method: GetWorkOrderParts - Failed")
                            Return String.Empty
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            objWalmartSC.WriteLine("GetWorkOrderParts- " + ex.Message.ToString())
            Return String.Empty
        End Try
    End Function

    Public Function DeleteWorkOrders(workOrder As String, ByVal objPartParam As Integer(), Session_THIRDPARTY_COMP_ID As String) As Boolean
        Try
            If Not String.IsNullOrEmpty(workOrder) And Not String.IsNullOrWhiteSpace(workOrder) Then
                Dim APIresponse = String.Empty
                If Session_THIRDPARTY_COMP_ID = ConfigurationManager.AppSettings("CBRECompanyID").ToString() Then
                    APIresponse = AuthenticateService("CBRE")
                Else
                    APIresponse = AuthenticateService("Walmart")
                End If
                If (APIresponse <> "Server Error" And APIresponse <> "Internet Error" And APIresponse <> "Error") Then
                    If (Not APIresponse.Contains("error_description")) Then
                        Dim objValidateUserResponseBO As ValidateUserResponseBO = JsonConvert.DeserializeObject(Of ValidateUserResponseBO)(APIresponse)
                        Dim apiURL = ConfigurationManager.AppSettings("ServiceChannelBaseAddress") + "/workorders/inventory/parts/bulkPartUsage"
                        Dim httpClient As HttpClient = New HttpClient()
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token)
                        Dim objDelete As New Delete
                        objDelete.DeleteItems = New List(Of DeleteItem)
                        For Each items As Integer In objPartParam
                            objDelete.DeleteItems.Add(New DeleteItem() With {
                                .PartId = items
                                })
                        Next
                        Dim serializedparameter = JsonConvert.SerializeObject(objDelete)
                        Dim response = httpClient.PostAsync(apiURL, New StringContent(serializedparameter, Encoding.UTF8, "application/json")).Result()
                        If response.IsSuccessStatusCode Then
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result()
                            objWalmartSC.WriteLine("Method: DeleteWorkOrders- Success")
                            Return True
                        Else
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result()
                            objWalmartSC.WriteLine("Method: DeleteWorkOrders-" + workorderAPIResponse.ToString())
                            Return False
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            objWalmartSC.WriteLine("Method: DeleteWorkOrders-" + ex.Message.ToString())
            Return False
        End Try
    End Function

    Public Function InsertWorkOrder(workOrder As String, cartDt As DataTable, Session_THIRDPARTY_COMP_ID As String) As Boolean
        Try
            If Not String.IsNullOrEmpty(workOrder) And Not String.IsNullOrWhiteSpace(workOrder) And cartDt.Rows.Count > 0 Then
                Dim APIresponse = String.Empty
                If Session_THIRDPARTY_COMP_ID = ConfigurationManager.AppSettings("CBRECompanyID").ToString() Then
                    APIresponse = AuthenticateService("CBRE")
                Else
                    APIresponse = AuthenticateService("Walmart")
                End If
                If (APIresponse <> "Server Error" And APIresponse <> "Internet Error" And APIresponse <> "Error") Then
                    If (Not APIresponse.Contains("error_description")) Then
                        Dim objValidateUserResponseBO As ValidateUserResponseBO = JsonConvert.DeserializeObject(Of ValidateUserResponseBO)(APIresponse)
                        Dim apiURL = ConfigurationManager.AppSettings("ServiceChannelBaseAddress") + "/workorders/inventory/parts/bulkPartUsage"
                        Dim httpClient As HttpClient = New HttpClient()
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                        httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token)
                        Dim objInserWorOrdeParts As New InsertWorkOrderPartsBO
                        objInserWorOrdeParts.AddItems = New List(Of AddItem)
                        'calling SC API to get store data and sending Scheduled date as Use Date
                        Dim useDate = DateTime.Now.ToString()
                        Dim objWorkOrderDetails As WorkOrderDetails
                        Dim apiURL1 = ConfigurationManager.AppSettings("ServiceChannelBaseAddress") + "/workorders/" + workOrder
                        Dim httpClient1 As HttpClient = New HttpClient()
                        httpClient1.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", objValidateUserResponseBO.access_token)
                        Dim response1 = httpClient1.GetAsync(apiURL1).Result
                        If response1.IsSuccessStatusCode Then
                            Dim workorderAPIResponse As String = response1.Content.ReadAsStringAsync().Result
                            If workorderAPIResponse <> "[]" And Not String.IsNullOrEmpty(workorderAPIResponse) And Not String.IsNullOrWhiteSpace(workorderAPIResponse) Then
                                objWorkOrderDetails = JsonConvert.DeserializeObject(Of WorkOrderDetails)(workorderAPIResponse)
                                useDate = objWorkOrderDetails.ScheduledDate
                            End If
                        End If
                        For Each item As DataRow In cartDt.Rows
                            Dim dueDate As String = String.Empty
                            Try
                                If Not String.IsNullOrEmpty(item("UDueDate").ToString()) Then
                                    dueDate = item("UDueDate").ToString().Split(" ")(0)
                                End If
                            Catch ddEx As Exception
                                dueDate = String.Empty
                            End Try

                            objInserWorOrdeParts.AddItems.Add(New AddItem() With {
                                .RecId = workOrder,
                                .UseDate = useDate,
                             .Description = item("ItemDescription") + " " + dueDate,
                             .Quantity = item("Quantity"),
                             .PartNumber = item("OrderNo")
                            })
                            '.UnitCost = item("Price"),
                        Next
                        Dim serializedparameter = JsonConvert.SerializeObject(objInserWorOrdeParts)
                        Dim response = httpClient.PostAsync(apiURL, New StringContent(serializedparameter, Encoding.UTF8, "application/json")).Result()
                        If response.IsSuccessStatusCode Then
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result()
                            objWalmartSC.WriteLine("Method: InsertWorkOrder- Success")
                            Return True
                        Else
                            Dim workorderAPIResponse As String = response.Content.ReadAsStringAsync().Result()
                            objWalmartSC.WriteLine("Method: InsertWorkOrder- " + workorderAPIResponse.ToString())
                            Return False
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            objWalmartSC.WriteLine("Method: InsertWorkOrder- " + ex.Message.ToString())
            Return False
        End Try
    End Function

    Public Function AuthenticateService(credType As String) As String
        Try
            Dim httpClient As HttpClient = New HttpClient()
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
            Dim username As String = String.Empty
            Dim password As String = String.Empty
            Dim clientKey As String = String.Empty
            If credType = "Walmart" Then
                username = ConfigurationManager.AppSettings("WMUName")
                password = ConfigurationManager.AppSettings("WMPassword")
                clientKey = ConfigurationManager.AppSettings("WMClientKey")
            Else
                username = ConfigurationManager.AppSettings("CBREUName")
                password = ConfigurationManager.AppSettings("CBREPassword")
                clientKey = ConfigurationManager.AppSettings("CBREClientKey")
            End If
            Dim apiurl As String = ConfigurationManager.AppSettings("ServiceChannelLoginEndPoint")
            Dim formContent = New FormUrlEncodedContent({New KeyValuePair(Of String, String)("username", username), New KeyValuePair(Of String, String)("password", password), New KeyValuePair(Of String, String)("grant_type", "password")})
            httpClient.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Basic", clientKey) 'Add("Authorization", "Basic " + clientKey)
            Dim response = httpClient.PostAsync(apiurl, formContent).Result
            If response.IsSuccessStatusCode Then
                Dim APIResponse = response.Content.ReadAsStringAsync().Result
                Return APIResponse
            Else
                Dim APIResponse = response.Content.ReadAsStringAsync().Result
                'Dim eobj As ExceptionHelper = New ExceptionHelper()
                'eobj.writeExceptionMessage(APIResponse, "AuthenticateService")
                If APIResponse.Contains("error_description") Then Return APIResponse
                Return "Server Error"
            End If

        Catch ex As Exception
            objWalmartSC.WriteLine("Method:AuthenticateService - " + ex.Message)
        End Try
        Return "Server Error"
    End Function
End Class

Public Class InsertWorkOrderPartsBO
    Public Property AddItems As List(Of AddItem)
    Public Property UpdateItems As List(Of Object) = New List(Of Object)
    Public Property DeleteItems As List(Of Object) = New List(Of Object)
    Public Property IsLocalTime As Boolean = True
End Class
Public Class ValidateUserResponseBO
    Public Property access_token As String
    Public Property refresh_token As String
End Class

Public Class AddItem
    Public Property RecId As String = String.Empty
    Public Property Quantity As Double
    Public Property UnitCost As Double
    Public Property UseDate As String = DateTime.Now.ToString()
    Public Property PartNumber As String = String.Empty
    Public Property Description As String = String.Empty
End Class

Public Class DeleteItem
    Public Property PartId As Integer
End Class

Public Class Delete
    Public Property DeleteItems As List(Of DeleteItem)
End Class

Public Class WorkOrderParts
    Public Property id As Integer
    Public Property Quantity As Double
    Public Property Description As String = String.Empty
    Public Property Price As Double
    Public Property SupplierPartId As String = String.Empty
End Class

Public Class WorkOrderDetails
    Public Property Notes As Notes
    Public Property Location As Location
    Public Property Asset As Asset
    Public Property PurchaseNumber As String = String.Empty
    Public Property ScheduledDate As String

End Class

Public Class Asset
    Public Property Tag As String = String.Empty
End Class
Public Class Location
    Public Property StoreId As String = String.Empty
End Class

Public Class Notes
    Public Property Last As Last
End Class

Public Class Last
    Public Property NoteData As String = String.Empty
End Class
