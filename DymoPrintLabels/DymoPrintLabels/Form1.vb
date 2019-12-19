Imports DYMO.Label.Framework


Public Class Form1

    Private Sub btnPrint_Click(sender As Object, e As EventArgs) Handles btnPrint.Click
        Dim iNumOfLbls As Integer = 1
        Dim myDymo As Object
        Dim myLabel As Object
        Dim sItemID As String = ""
        Dim sDescr As String = ""
        'Dim sItemID As String = ""

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
