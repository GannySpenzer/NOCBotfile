Public Class WebForm1
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim iNumOfLbls As Integer = 1
        Dim myDymo As Object
        Dim myLabel As Object
        Dim sItemID As String = ""
        Dim sDescr As String = ""
        'Dim sItemID As String = ""

        Dim zz As New DYMO.DymoAddIn()
        Dim yy As New Dymo.DymoLabels()

        yy.SetField("ItemID_1", sItemID)
        yy.SetField("Description", sDescr)
        zz.StartPrintJob()
        zz.Print(1, True)


        'test new line here
        sItemID = "C100004334"
        sDescr = "Some Label Description"

        myDymo = CreateObject("Dymo.Dymoaddin")

        myLabel = CreateObject("Dymo.Dymolabels")

        myDymo.Open(CStr("C:\\Program Files\\Dymo Label\\Label Files\\WebPartner Label6(new).LWT"))

        myLabel.SetField("ItemID_1", sItemID)

        myLabel.SetField("Description", sDescr)

        myDymo.Print(iNumOfLbls, True)
    End Sub

End Class