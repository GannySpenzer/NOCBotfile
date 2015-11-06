Option Strict On

Imports System.ComponentModel
Imports System.Reflection
Imports System.Drawing
Imports System.Drawing.Printing
Imports System.Web
Imports System.Text.RegularExpressions


Public Class shippingLabel

    Inherits PrintDocument

    Private Const oraCN_default_provider As String = "Provider=MSDAORA.1;"
    Private Const oraCN_default_creden As String = "User ID=einternet;Password=einternet;"
    Private Const oraCN_default_DB As String = "Data Source=prod"

    ''This line was added to allow printing as a DLL
    'Protected WithEvents LinearBarcode1 As IDAutomation.LinearServerControl.LinearBarcode
    Private WithEvents linearBarcoder As IDAutomation.LinearServerControl.LinearBarcode


#Region " Event declarations "


    ''' <summary>
    ''' Raised once immediately before anything is printed to the report. The cursor is on the first line of the first page.
    ''' </summary>
    Public Event ReportBegin(ByVal sender As Object, _
                             ByVal e As shippingLabelPageEventArgs)
    ''' <summary>
    ''' Raised for each page immediately before anything is printed to that page. The cursor is on the first line of the page.
    ''' </summary>
    Public Event PrintPageBegin(ByVal sender As Object, _
                                ByVal e As shippingLabelPageEventArgs)
    ''' <summary>
    ''' Raised for each page immediately after the header for the page has been printed. The cursor is on the first line of the report body.
    ''' </summary>
    Public Event PrintPageBodyEnd(ByVal sender As Object, _
                                  ByVal e As shippingLabelPageEventArgs)
    ''' <summary>
    ''' Raised for each page after the footer has been printed. The cursor is past the end of the footer, typically into the bottom margin of the page.
    ''' </summary>
    Public Event PrintPageEnd(ByVal sender As Object, _
                              ByVal e As shippingLabelPageEventArgs)
    ''' <summary>
    ''' Raised once at the very end of the report after all other printing is complete. The cursor is past the end of the footer on the last page, typically into the bottom margin of the page.
    ''' </summary>
    Public Event ReportEnd(ByVal sender As Object, _
                           ByVal e As shippingLabelPageEventArgs)

#End Region

#Region " Report Properties and Settings "

    Private mPageNumber As Integer
    Private mPageLine As Integer
    Private mTitle As String
    Private mSubTitleLeft As String
    Private mSubTitleLeft1 As String
    Private mSubTitleRight As String
    Private mFooterLeft As String
    Private mFooterRight As String
    Private mFont As Font
    Private mBrush As Brush
    Private mFooterLines As Integer
    Private mSrcBU As String
    'Private mOverRide As String
    Private mOrderNo As String
    'Private mDB As String
    Private arial30B As New Font("Arial", 30, FontStyle.Bold)
    Private arial20B As New Font("Arial", 20, FontStyle.Bold)
    Private arial18B As New Font("Arial", 18, FontStyle.Bold)
    Private arial8 As New Font("Arial", 8)
    Private arial8B As New Font("Arial", 8, FontStyle.Bold)
    Private arial9 As New Font("Arial", 8)
    Private arial10 As New Font("Arial", 10)
    Private arial10B As New Font("Arial", 10, FontStyle.Bold)
    Private arial12 As New Font("Arial", 12)
    Private arial12B As New Font("Arial", 12, FontStyle.Bold)
    Private ds As New DataSet
    Private ds1 As New DataSet
    Private ds2 As New DataSet
    Private ds3 As New DataSet
    Private arrItemIDC As ArrayList
    Private strPrevCntrID As String
    Private strPrevCntrIDFooter As String
    Private m_bIsIgnoreShipDTTM As Boolean = False
    Private m_logger As SDI.ApplicationLogger.IApplicationLogger = New SDI.ApplicationLogger.noAppLogger
    Private m_oraCNstring As String = "" & _
                                      oraCN_default_provider & _
                                      oraCN_default_creden & _
                                      oraCN_default_DB & _
                                      ""
    Private m_pathSQLs As String = ""
    Private m_executionPath As String = ""


#Region " Constructors "

    Public Sub New()

        MyBase.New()
        InitMembers()

    End Sub

    ' retained for compatibility purposes
    '   these params are now exposed as properties
    Public Sub New(ByVal srcbu As String, ByVal orderNo As String, ByVal strDB As String)

        MyBase.New()
        InitMembers()

        mSrcBU = srcbu
        mOrderNo = orderNo
        'mDB = strDB
        m_oraCNstring = "" & _
                        oraCN_default_provider & _
                        oraCN_default_creden & _
                        "Data Source=" & strDB & _
                        ""

    End Sub

    ' retained for compatibility purposes
    '   these params are now exposed as properties
    Public Sub New(ByVal srcbu As String, ByVal orderNo As String, ByVal strDB As String, ByVal strShipOverRide As String)

        MyBase.New()
        InitMembers()

        mSrcBU = srcbu
        'mOverRide = strShipOverRide
        mOrderNo = orderNo
        'mDB = strDB
        m_oraCNstring = "" & _
                        oraCN_default_provider & _
                        oraCN_default_creden & _
                        "Data Source=" & strDB & _
                        ""
        'mOverRide = strShipOverRide
        If Not (strShipOverRide Is Nothing) Then
            If strShipOverRide.Trim.Length > 0 Then
                m_bIsIgnoreShipDTTM = (strShipOverRide.Trim.ToUpper = "Y")
            End If
        End If

    End Sub

    Private Sub InitMembers()

        MyBase.DefaultPageSettings.Margins.Top = 10
        MyBase.DefaultPageSettings.Margins.Left = 10
        MyBase.DefaultPageSettings.Margins.Right = 10
        MyBase.DefaultPageSettings.Landscape = True

        mFont = New Font("Arial", 10)
        mBrush = Brushes.Black

        FooterLines = 2

        arrItemIDC = New ArrayList

        ' get the executing assembly folder
        Dim asmMe As String = System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName
        If asmMe.Length > 0 Then
            asmMe = asmMe.Substring(startIndex:=0, length:=asmMe.Length - (asmMe.Length - asmMe.LastIndexOf(CType("\", Char))))
        Else
            asmMe = ""
        End If
        asmMe &= "\"
        m_executionPath = asmMe

        Dim ass As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly
        Dim assName As System.Reflection.AssemblyName = ass.GetName
        Dim dllName As String = assName.Name

        'm_pathSQLs = asmMe & dllName & ".SQLs\"
        m_pathSQLs = m_executionPath & dllName & ".SQLs\"

    End Sub

#End Region


    'Component overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Component Designer
    'It can be modified using the Component Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        '
        'shippingLabel
        '
    End Sub

    Public Property SourceBusinessUnit() As String
        Get
            Return mSrcBU
        End Get
        Set(ByVal Value As String)
            mSrcBU = Value
        End Set
    End Property

    Public Property OrderNo() As String
        Get
            Return mOrderNo
        End Get
        Set(ByVal Value As String)
            mOrderNo = Value
        End Set
    End Property

    Public Property oraCNstring() As String
        Get
            Return m_oraCNstring
        End Get
        Set(ByVal Value As String)
            m_oraCNstring = Value
        End Set
    End Property

    Public Property IsIgnoreShipDTTM() As Boolean
        Get
            Return m_bIsIgnoreShipDTTM
        End Get
        Set(ByVal Value As Boolean)
            m_bIsIgnoreShipDTTM = Value
        End Set
    End Property

    Public Property Logger() As SDI.ApplicationLogger.IApplicationLogger
        Get
            If (m_logger Is Nothing) Then
                m_logger = New SDI.ApplicationLogger.noAppLogger
            End If
            Return m_logger
        End Get
        Set(ByVal Value As SDI.ApplicationLogger.IApplicationLogger)
            m_logger = Value
        End Set
    End Property

    Public Property ExecutionPath() As String
        Get
            Return m_executionPath
        End Get
        Set(ByVal Value As String)
            ' override if path was specified
            If Value.Trim.Length > 0 Then
                m_executionPath = Value
                Dim ass As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly
                Dim assName As System.Reflection.AssemblyName = ass.GetName
                Dim dllName As String = assName.Name
                m_pathSQLs = m_executionPath & dllName & ".SQLs\"
            End If
        End Set
    End Property

    ''' <summary>
    ''' Allows the developer to set or retrieve the Font object that is used
    ''' to render the text of the report. This defaults to a 10 point
    ''' Courier New font.
    ''' </summary>
    ''' <value>A Font object</value>
    Public Property Font() As Font
        Get
            Return mFont
        End Get
        Set(ByVal Value As Font)
            mFont = Value
        End Set
    End Property


    ''' <summary>
    ''' Allows the developer to set or retrieve the Brush object that is
    ''' used to render the text of the report. This defaults to a solid black
    ''' brush.
    ''' </summary>
    ''' <value>A Brush object</value>
    Public Property Brush() As Brush
        Get
            Return mBrush
        End Get
        Set(ByVal Value As Brush)
            mBrush = Value
        End Set
    End Property

    ''' <summary>
    ''' Sets or returns the number of lines reserved at the bottom of each page
    ''' for the footer. This defaults to 2 lines for the default footer. If you
    ''' want to add extra lines to the footer you should increase this value accordingly.
    ''' </summary>
    ''' <value>The number of lines reserved for the page footer.</value>
    Public Property FooterLines() As Integer
        Get
            Return mFooterLines
        End Get
        Set(ByVal Value As Integer)
            mFooterLines = Value
        End Set
    End Property

    ''' <summary>
    ''' The report title displayed at the top of each page.
    ''' </summary>
    ''' <value>Text to be displayed.</value>
    Public Property Title() As String
        Get
            Return mTitle
        End Get
        Set(ByVal Value As String)
            mTitle = Value
        End Set
    End Property

    ''' <summary>
    ''' Text to be displayed on the left side of the line below the title on each page.
    ''' </summary>
    ''' <value>Text to be displayed.</value>
    Public Property SubTitleLeft() As String
        Get
            Return mSubTitleLeft
        End Get
        Set(ByVal Value As String)
            mSubTitleLeft = Value
        End Set
    End Property

    ''' <summary>
    ''' Text to be displayed on the left side of the line below the title on each page.
    ''' </summary>
    ''' <value>Text to be displayed.</value>
    Public Property SubTitleLeft1() As String
        Get
            Return mSubTitleLeft1
        End Get
        Set(ByVal Value As String)
            mSubTitleLeft1 = Value
        End Set
    End Property

    ''' <summary>
    ''' Text to be displayed on the right side of the line below the title on each page.
    ''' </summary>
    ''' <value>Text to be displayed.</value>
    Public Property SubTitleRight() As String
        Get
            Return mSubTitleRight
        End Get
        Set(ByVal Value As String)
            mSubTitleRight = Value
        End Set
    End Property

    ''' <summary>
    ''' Text to be displayed on the left side of the footer below the separator line
    ''' at the bottom of each page.
    ''' </summary>
    ''' <value>Text to be displayed.</value>
    Public Property FooterLeft() As String
        Get
            Return mFooterLeft
        End Get
        Set(ByVal Value As String)
            mFooterLeft = Value
        End Set
    End Property

    ''' <summary>
    ''' Text to be displayed on the right side of the footer below the separator line
    ''' at the bottom of each page.
    ''' </summary>
    ''' <value>Text to be displayed.</value>
    Public Property FooterRight() As String
        Get
            Return mFooterRight
        End Get
        Set(ByVal Value As String)
            mFooterRight = Value
        End Set
    End Property

#End Region

#Region " Do printing "

    Dim mRow As Integer

    Private Sub ReportDocument1_BeginPrint(ByVal sender As Object, _
                                           ByVal e As System.Drawing.Printing.PrintEventArgs) Handles MyBase.BeginPrint

        mPageNumber = 0
        mPageLine = 0
        mRow = 0
        buildDatasets()

    End Sub

    Private Sub ReportDocument1_PrintPage(ByVal sender As Object, _
                                          ByVal e As System.Drawing.Printing.PrintPageEventArgs) Handles MyBase.PrintPage

        mPageNumber += 1
        mPageLine = 1

        ' create our ReportPageEventArgs1 object for this page
        Dim page As New shippingLabelPageEventArgs(e, mPageNumber, mFont, mBrush, mFooterLines)

        ' if we're generating page 1 raise the ReportBegin event
        If mPageNumber = 1 Then
            RaiseEvent ReportBegin(Me, page)
        End If

        ' generate the page header/body/foote

        GeneratePage(page)

        ' if there are no more pages to generate then raise
        ' the ReportEnd event
        If Not page.HasMorePages Then
            RaiseEvent ReportEnd(Me, page)
        End If

        ' the client code may have overridden the Cancel or
        ' HasMorePages values somewhere during the process,
        ' so we restore them to the underlying PrintPageEventArgs
        ' object - thus allowing our base class to take care
        ' of these details for us
        e.Cancel = page.Cancel
        e.HasMorePages = page.HasMorePages

    End Sub

    Private Sub PrintPageBodyStart(ByVal e As shippingLabelPageEventArgs)

        Dim strSQLstring As String
        Dim I As Integer
        Dim X As Integer
        Dim Z As Integer
        Dim arrNotes As ArrayList
        Dim strFoot1 As String
        Dim strFoot2 As String
        Dim strFoot3 As String
        Dim strShipToDescr As String
        Dim strpreviousShipContainer As String
        Dim strpreviousLine As String

        Dim strcurrentShipContainer As String
        Dim intSavemY As Integer
        Dim intStartMY As Integer

        ' generate the header
        PrintHeader(e)

        With e
            If ds.Tables(0).Rows.Count = 0 Then
                .CurrentF = arial10B
                .WriteLine("No data found for this order number! - " & mOrderNo, shippingLabelRptLineJustification.Left)
                Exit Sub
            End If

            .CurrentF = arial10
            .CurrentH = arial10.Height

            .WriteLine()

            DrawRectangleFloat(e, .CurrentX, .CurrentY, 650, 625)
            .CurrentY2 = .CurrentY + 635

            DrawRectangleFloat(e, .CurrentX, .CurrentY2, 650, 65)
            .CurrentY += 5
            .CurrentF = arial10B
            .CurrentH = arial10B.Height
            .Write("Line-Sched", shippingLabelRptLineJustification.colLineSched)
            .Write("Product Number", shippingLabelRptLineJustification.colProductNumber)
            .Write("Ship Date", shippingLabelRptLineJustification.colShipDate)
            .Write("UOM", shippingLabelRptLineJustification.colUOM)
            .Write("Qty", shippingLabelRptLineJustification.colQtyOrdered)
            .Write("Qty", shippingLabelRptLineJustification.colQtyShipped)
            .WriteLine(" ", shippingLabelRptLineJustification.colQtyBackorder)

            .Write("Description", shippingLabelRptLineJustification.colProductNumber)
            .Write("Ordered:", shippingLabelRptLineJustification.colQtyOrdered)
            .Write("Shipped:", shippingLabelRptLineJustification.colQtyShipped)
            .WriteLine(" ", shippingLabelRptLineJustification.colQtyBackorder)
            .HorizontalRule()
            'arrNotes = FormatNotes(e, ds.Tables(0).Rows(I).Item("ISA_CUST_NOTES").toupper, arial8)
            arrNotes = FormatWrap(e, CStr(ds1.Tables(0).Rows(0).Item("ISA_CUST_NOTES")).ToUpper, arial8, 640)

            intSavemY = .CurrentY
            .CurrentY = .CurrentY2
            .CurrentF = arial10B
            .CurrentH = arial10B.Height
            .WriteLine("Delivery Notes:", shippingLabelRptLineJustification.Left)
            .CurrentF = arial8
            .CurrentH = arial8.Height
            For I = 0 To arrNotes.Count - 1
                .WriteLine(CStr(arrNotes(I)))
            Next
            .CurrentY = intSavemY
            .CurrentF = arial10
            .CurrentH = arial10.Height

            Dim arrItemIDA As ArrayList
            Dim arrItemIDB As ArrayList
            Dim intIndex As Integer

            arrItemIDA = New ArrayList
            arrItemIDB = New ArrayList

            If ds.Tables(0).Rows.Count > 0 Then

                For I = 0 To ds.Tables(0).Rows.Count - 1
                    intIndex = arrItemIDA.IndexOf(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")) & CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")))
                    If intIndex = -1 Then
                        arrItemIDA.Add(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")) & CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")))
                    Else
                        intIndex = arrItemIDB.IndexOf(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")) & CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")))
                        If intIndex = -1 Then
                            arrItemIDB.Add(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")) & CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")))
                        End If
                    End If
                Next

                If IsDBNull(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID")) Then
                    strcurrentShipContainer = " "
                Else
                    strcurrentShipContainer = CStr(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID"))
                End If
                strpreviousShipContainer = strcurrentShipContainer
                For I = mRow To ds.Tables(0).Rows.Count - 1
                    intStartMY = .CurrentY
                    Dim arrDescr As ArrayList
                    arrDescr = FormatWrap(e, Regex.Replace(CStr(ds.Tables(0).Rows(I).Item("DESCR254")), "\s+", " ").ToUpper, arial10, 530)
                    If IsDBNull(ds.Tables(0).Rows(I).Item("SHIP_CNTR_ID")) Then
                        strcurrentShipContainer = " "
                    Else
                        strcurrentShipContainer = CStr(ds.Tables(0).Rows(I).Item("SHIP_CNTR_ID"))
                    End If
                    'If .CurrentY + .CurrentS + (.CurrentS * arrDescr.Count) > .CurrentY2 - .CurrentS Then
                    If .CurrentY + .CurrentS + (.CurrentS * arrDescr.Count) > .CurrentY2 + .CurrentS Then
                        e.HasMorePages = True
                        Exit Sub
                    ElseIf strcurrentShipContainer <> strpreviousShipContainer Then
                        'mPageNumber -= 1
                        e.HasMorePages = True
                        Exit Sub
                    End If

                    ' get qty req/alloc
                    Dim myQty As New demandQty
                    Try
                        myQty = demandQty.GetDemandQty(sourceBU:=CStr(ds.Tables(0).Rows(I).Item("SOURCE_BUS_UNIT")), _
                                                       OrderNo:=CStr(ds.Tables(0).Rows(I).Item("ORDER_NO")), _
                                                       demandSource:=CStr(ds.Tables(0).Rows(I).Item("DEMAND_SOURCE")), _
                                                       schedLineNo:=CInt(ds.Tables(0).Rows(I).Item("SCHED_LINE_NO")), _
                                                       itemId:=CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")), _
                                                       pathSQLs:=m_pathSQLs, _
                                                       oraCNstring:=Me.oraCNstring)
                    Catch ex As Exception
                    End Try

                    Dim nOrderedAndPickedDiff As Integer = 0
                    Dim idx As Integer = arrItemIDC.IndexOf(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")) & CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")))
                    If idx = -1 Then
                        Try
                            'nOrderedAndPickedDiff = (CInt(ds.Tables(0).Rows(I).Item("QTY_ORDERED")) - CInt(ds.Tables(0).Rows(I).Item("QTY_PICKED")))
                            nOrderedAndPickedDiff = (myQty.QtyRequested - CInt(ds.Tables(0).Rows(I).Item("QTY_PICKED")))
                        Catch ex As Exception
                        End Try
                    End If

                    If Not (mPageLine Mod 2) = 0 Then
                        '.HorizontalShading(intStartMY, arrDescr.Count)
                        .HorizontalShading(intStartMY, arrDescr.Count + CInt(IIf(nOrderedAndPickedDiff < 0, 1, 0)))
                    End If
                    strpreviousShipContainer = strcurrentShipContainer

                    .Write((CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")) & " - " & CStr(ds.Tables(0).Rows(I).Item("SCHED_LINE_NO"))), shippingLabelRptLineJustification.colLineSched)
                    intIndex = arrItemIDB.IndexOf(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")) & CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")))
                    If intIndex > -1 Then
                        .Write("*")
                    End If
                    .Write(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")), shippingLabelRptLineJustification.colProductNumber)
                    .Write(Convert.ToString(ds.Tables(0).Rows(I).Item("pick_date")), shippingLabelRptLineJustification.colShipDate)
                    .Write(CStr(ds.Tables(0).Rows(I).Item("UNIT_OF_MEASURE")), shippingLabelRptLineJustification.colUOM)
                    intIndex = arrItemIDC.IndexOf(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")) & CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")))
                    If intIndex = -1 Then
                        '.Write(CStr(ds.Tables(0).Rows(I).Item("QTY_ORDERED")), shippingLabelRptLineJustification.colQtyOrdered)
                        .Write(CStr(myQty.QtyRequested), shippingLabelRptLineJustification.colQtyOrdered)
                        .Write(CStr(ds.Tables(0).Rows(I).Item("QTY_PICKED")), shippingLabelRptLineJustification.colQtyShipped)
                        '.WriteLine(CStr(CDbl(ds.Tables(0).Rows(I).Item("QTY_ORDERED")) - CDbl(ds.Tables(0).Rows(I).Item("QTY_PICKED"))), _
                        '           shippingLabelRptLineJustification.colQtyBackorder)
                        .WriteLine(CStr(CDbl(myQty.QtyRequested) - CDbl(ds.Tables(0).Rows(I).Item("QTY_PICKED"))), _
                                   shippingLabelRptLineJustification.colQtyBackorder)
                    Else
                        .Write("--", shippingLabelRptLineJustification.colQtyOrdered)
                        .Write("--", shippingLabelRptLineJustification.colQtyShipped)
                        .WriteLine("--", shippingLabelRptLineJustification.colQtyBackorder)
                    End If

                    Dim sNotice As String = ""

                    If nOrderedAndPickedDiff < 0 Then
                        sNotice = "" & _
                                  "*** WARNING:  PLEASE NOTE PICKED QTY IS GREATER THAN REQUESTED ***" & _
                                  ""
                    End If

                    intIndex = arrItemIDB.IndexOf(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")) & CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")))
                    If intIndex > -1 Then
                        intIndex = arrItemIDC.IndexOf(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")) & CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")))
                        If intIndex = -1 Then
                            arrItemIDC.Add(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")) & CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")))
                        End If
                    End If

                    '' the descr should wrap bur for now substring to 60
                    'If ds.Tables(0).Rows(I).Item("DESCR254").length > 60 Then
                    '    .WriteLine(Convert.ToString(ds.Tables(0).Rows(I).Item("DESCR254")).Substring(0, 60), shippingLabelRptLineJustification.colProductNumber)
                    'Else
                    '    .WriteLine(ds.Tables(0).Rows(I).Item("DESCR254"), shippingLabelRptLineJustification.colProductNumber)
                    'End If

                    For X = 0 To arrDescr.Count - 1
                        .WriteLine(CStr(arrDescr(X)), shippingLabelRptLineJustification.colProductNumber)
                    Next

                    If sNotice.Trim.Length > 0 Then
                        Dim myF As System.Drawing.Font = .CurrentF
                        Dim myH As Integer = .CurrentH
                        .CurrentF = arial8
                        .CurrentH = arial8.Height
                        .WriteLine(sNotice, shippingLabelRptLineJustification.colLineSched)
                        .CurrentF = myF
                        .CurrentH = myH
                    End If

                    .WriteLine()
                    mRow += 1
                    mPageLine += 1
                Next

            End If

        End With


    End Sub


    Private Sub GeneratePage(ByVal e As shippingLabelPageEventArgs)

        ' we're about to print the page
        RaiseEvent PrintPageBegin(Me, e)

        ' we're about to print the body of the page
        PrintPageBodyStart(e)

        ' we're done generating the body of this page
        RaiseEvent PrintPageBodyEnd(Me, e)

        ' generate the page footer unless it is supressed
        PrintFooter(e)

        ' we're all done with the page
        RaiseEvent PrintPageEnd(Me, e)

    End Sub

    Private Function FormatWrap(ByVal e As shippingLabelPageEventArgs, ByVal currentText As String, ByVal pFont As Font, ByVal fieldsize As Integer) As ArrayList

        Dim lengthOfText As Integer
        Dim maxLengthOfALine As Integer
        Dim startingPosition As Integer
        Dim endingPosition As Integer
        Dim lineLength As Integer
        Dim line As String
        Dim decDiv As Decimal
        Dim I As Integer
        Dim arrText As ArrayList
        arrText = New ArrayList

        endingPosition = currentText.Length
        lengthOfText = CInt(e.Graphics.MeasureString(currentText, pFont).Width)
        decDiv = CDec(lengthOfText / fieldsize)
        maxLengthOfALine = CInt(endingPosition / decDiv)
        lineLength = maxLengthOfALine
        If endingPosition < maxLengthOfALine Then
            arrText.Add(currentText)
            Return arrText
        End If
        ' start looping through the text until
        While lineLength + startingPosition <= endingPosition
            While currentText.Substring(startingPosition + lineLength - 1, 1) <> Chr(32)
                lineLength -= 1
                If lineLength < (maxLengthOfALine - 9) Then
                    lineLength = lineLength + 8
                    Exit While
                End If
            End While
            line = currentText.Substring(startingPosition, lineLength)
            arrText.Add(line)
            startingPosition += line.Length
            lineLength = maxLengthOfALine
        End While
        If startingPosition + lineLength > endingPosition Then
            line = currentText.Substring(startingPosition)
            arrText.Add(line)
        End If

        FormatWrap = arrText

    End Function

    Private Function getShipto(ByVal strShipto As String) As DataSet

        Dim rtn As String = "shippingLabel.getShipto"

        Dim sql As SQLBuilder = Nothing

        'Dim strSQLstring As String = "SELECT A.DESCR, A.ADDRESS1, A.ADDRESS2," & vbCrLf & _
        '            " A.ADDRESS3, A.ADDRESS4, A.CITY, A.STATE, A.POSTAL" & vbCrLf & _
        '            " FROM PS_LOCATION_TBL A" & vbCrLf & _
        '            " WHERE A.EFFDT =" & vbCrLf & _
        '            " (SELECT MAX(A_ED.EFFDT) FROM PS_LOCATION_TBL A_ED" & vbCrLf & _
        '            " WHERE A.SETID = A_ED.SETID" & vbCrLf & _
        '            " AND A.LOCATION = A_ED.LOCATION" & vbCrLf & _
        '            " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
        '            " AND A.LOCATION = '" & strShipto & "'" & vbCrLf & _
        '            " AND A.EFF_STATUS = 'A'"

        sql = New SQLBuilder(myCommon.LoadQuery(m_pathSQLs & "getLocAddress_InvTransfer_SELECT.sql"))
        sql.Parameters.Add(":KEY_LOCATION", strShipto)

        LogVerbose(msg:=rtn & "::CN string : " & Me.oraCNstring)
        LogVerbose(msg:=rtn & "::executing : " & sql.ToString)

        'Dim dbConn As String = _
        '"Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=" & mDB

        ''"data source=ineroth;initial catalog=pubs;integrated security=SSPI"
        'Dim da As New OleDb.OleDbDataAdapter(strSQLstring, dbConn)
        Dim da As New OleDb.OleDbDataAdapter(sql.ToString, Me.oraCNstring)
        Dim dsAddr As New DataSet
        da.Fill(dsAddr)

        sql = Nothing

        Return dsAddr

    End Function

    Private Function getTextLength(ByVal e As shippingLabelPageEventArgs, ByVal currentText As String, ByVal pFont As Font) As Integer

        getTextLength = CInt(e.Graphics.MeasureString(currentText, pFont).Width)

    End Function

    Private Sub PrintHeader(ByVal e As shippingLabelPageEventArgs)

        Dim rtn As String = "shippingLabel.PrintHeader"

        'Dim strSQLstring As String
        Dim intContainers As Integer
        Dim strShipToDescr As String
        Dim strShipToAddress1 As String
        Dim strShipToAddress2 As String
        Dim strShipToAddress3 As String
        Dim strShipToAddress4 As String
        Dim strShipToCityState As String
        Dim bolCityState As Boolean
        Dim intStartY As Integer
        Dim intEndY As Integer
        Dim I As Integer

        If e.PageNumber = 1 Then

            '    Dim dbConn As String = _
            '"Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=" & mDB

            Dim sql As SQLBuilder = Nothing

            'strSQLstring = "SELECT DISTINCT B.SHIP_CNTR_ID" & vbCrLf & _
            '            " FROM PS_ISA_PICKING_INT A, PS_ISA_PICKING_CNT B" & vbCrLf & _
            '            " WHERE A.SOURCE_BUS_UNIT = '" & mSrcBU & "'" & vbCrLf & _
            '            " AND A.ORDER_NO = '" & mOrderNo & "'" & vbCrLf & _
            '            " AND NOT A.ENDDTTM IS NULL" & vbCrLf & _
            '            " AND A.QTY_PICKED > 0" & vbCrLf & _
            '            " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
            '            " AND A.DEMAND_SOURCE = B.DEMAND_SOURCE" & vbCrLf & _
            '            " AND A.SOURCE_BUS_UNIT = B.SOURCE_BUS_UNIT" & vbCrLf & _
            '            " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
            '            " AND A.ORDER_INT_LINE_NO = B.ORDER_INT_LINE_NO" & vbCrLf & _
            '            " AND A.SCHED_LINE_NO = B.SCHED_LINE_NO" & vbCrLf & _
            '            " AND A.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf & _
            '            " AND A.DEMAND_LINE_NO = B.DEMAND_LINE_NO" & vbCrLf & _
            '            " AND A.SEQ_NBR = B.SEQ_NBR" & vbCrLf & _
            '            " AND A.RECEIVER_ID = B.RECEIVER_ID" & vbCrLf & _
            '            " AND A.RECV_LN_NBR = B.RECV_LN_NBR" & vbCrLf

            sql = New SQLBuilder(myCommon.LoadQuery(m_pathSQLs & "getContainerInfo_InvTransfer_SELECT.sql"))
            sql.Parameters.Add(":KEY_SOURCE_BU", mSrcBU)
            sql.Parameters.Add(":KEY_ORDER_NO", mOrderNo)

            LogVerbose(msg:=rtn & "::CN string : " & Me.oraCNstring)
            LogVerbose(msg:=rtn & "::executing : " & sql.ToString)

            'Dim da3 As New OleDb.OleDbDataAdapter(strSQLstring, dbConn)
            Dim da3 As New OleDb.OleDbDataAdapter(sql.ToString, Me.oraCNstring)

            da3.Fill(ds3)
            intContainers = ds3.Tables(0).Rows.Count

            sql = Nothing

        End If

        With e

            .RotatePrint()

            If ds.Tables(0).Rows.Count = 0 Then
                .CurrentF = arial10B
                .WriteLine("No data found for this order number! - " & mOrderNo, shippingLabelRptLineJustification.Centered)
                Exit Sub
            End If
            intStartY = e.CurrentY
            DrawRectangleFloat(e, 675, .CurrentY, 390, 510)
            .CurrentF = arial30B
            .CurrentH = arial30B.Height
            .Write("SDI, Inc. ", shippingLabelRptLineJustification.Left)
            .CurrentF = arial20B
            .Write("Packing Slip", 425)
            .WriteLine()
            .CurrentF = arial8
            .CurrentH = arial8.Height
            .Write("The Power of Integrated Supply", shippingLabelRptLineJustification.Left)
            .WriteLine()
            intEndY = .CurrentY

            ' display a horizontal line to separate the header from
            ' the body
            '.HorizontalRule()

            .CurrentY = intStartY
            .CurrentF = arial18B
            .CurrentH = arial18B.Height

            If ds2.Tables(0).Rows.Count > 0 Then
                If mSrcBU = "I0206" Then 'gez 10/10/2012
                    strShipToDescr = CStr(ds1.Tables(0).Rows(0).Item("ISA_EMPLOYEE_NAME"))  'gez 10/10/2012
                    strShipToAddress1 = "" 'gez 10/10/2012
                    strShipToAddress2 = "" 'gez 10/10/2012
                    strShipToAddress3 = "" 'gez 10/10/2012
                    strShipToAddress4 = "" 'gez 10/10/2012r 
                    strShipToCityState = "" 'gez 10/10/2012
                Else 'gez 10/10/2012
                    strShipToDescr = CStr(ds2.Tables(0).Rows(0).Item("DESCR"))
                    strShipToAddress1 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS1"))
                    strShipToAddress2 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS2"))
                    strShipToAddress3 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS3"))
                    strShipToAddress4 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS4"))
                    strShipToCityState = CStr(ds2.Tables(0).Rows(0).Item("CITY")) & ", " & _
                    CStr(ds2.Tables(0).Rows(0).Item("STATE")) & " " & _
                    CStr(ds2.Tables(0).Rows(0).Item("POSTAL"))

                End If 'gez 10/10/2012
            End If

            'If ds2.Tables(0).Rows.Count > 0 Then
            '    strShipToDescr = CStr(ds2.Tables(0).Rows(0).Item("DESCR"))
            '    strShipToAddress1 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS1"))
            '    strShipToAddress2 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS2"))
            '    strShipToAddress3 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS3"))
            '    strShipToAddress4 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS4"))
            '    strShipToCityState = CStr(ds2.Tables(0).Rows(0).Item("CITY")) & ", " & _
            '                         CStr(ds2.Tables(0).Rows(0).Item("STATE")) & " " & _
            '                         CStr(ds2.Tables(0).Rows(0).Item("POSTAL"))

            'End If

            .CurrentF = arial30B
            .CurrentH = arial30B.Height
            .Write("SDI, Inc. ", shippingLabelRptLineJustification.col6)
            .WriteLine()
            .CurrentF = arial8
            .CurrentH = arial8.Height
            .Write("The Power of Integrated Supply", shippingLabelRptLineJustification.col6)
            .WriteLine()
            .WriteLine()
            .CurrentF = arial20B
            .CurrentH = arial20B.Height

            If IsDBNull(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID")) Then
                .Write("Items are back ordered.", shippingLabelRptLineJustification.col6)
                .WriteLine()
                .CurrentY = intEndY
                Exit Sub
            End If
            For I = 0 To ds3.Tables(0).Rows.Count - 1
                If CStr(ds3.Tables(0).Rows(I).Item("SHIP_CNTR_ID")) = CStr(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID")) Then
                    Exit For
                End If
            Next

            .Write("Package " & (I + 1) & " of " & ds3.Tables(0).Rows.Count, shippingLabelRptLineJustification.col6)
            .WriteLine()
            If strPrevCntrID <> CStr(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID")) Then

                .CurrentH = arial8.Height
                .WriteLine()
                .CurrentH = arial20B.Height
                .Write(CStr(ds1.Tables(0).Rows(0).Item("SHIPTO_ID")), shippingLabelRptLineJustification.col6)
                .WriteLine()
                .CurrentH = arial8.Height
                .WriteLine()
                .CurrentH = arial20B.Height
                .Write("Work Order:", shippingLabelRptLineJustification.col6)
                .Write(CStr(ds.Tables(0).Rows(mRow).Item("ISA_WORK_ORDER_NO")))
                .WriteLine()
                .CurrentH = arial8.Height
                .WriteLine()
                .CurrentF = arial12B
                .CurrentH = arial12B.Height

                
                .Write("Deliver To...:2", shippingLabelRptLineJustification.col6)
                If mSrcBU = "I0206" Then 'gez 10/10/2012
                    .WriteLine(strShipToDescr, shippingLabelRptLineJustification.col7)
                Else
                    .WriteLine(strShipToAddress1, shippingLabelRptLineJustification.col7)
                End If
                .WriteLine(strShipToAddress1, shippingLabelRptLineJustification.col7)
                If Not Trim(strShipToAddress2) = "" Then
                    .WriteLine(strShipToAddress2, shippingLabelRptLineJustification.col7)
                ElseIf Not Trim(strShipToAddress3) = "" Then
                    .WriteLine(strShipToAddress3, shippingLabelRptLineJustification.col7)
                ElseIf Not Trim(strShipToAddress4) = "" Then
                    .WriteLine(strShipToAddress4, shippingLabelRptLineJustification.col7)
                Else
                    .WriteLine(strShipToCityState, shippingLabelRptLineJustification.col7)
                    bolCityState = True
                End If

                If bolCityState = False Then
                    If Not Trim(strShipToAddress3) = "" Then
                        .WriteLine(strShipToAddress3, shippingLabelRptLineJustification.col7)
                    ElseIf Not Trim(strShipToAddress4) = "" Then
                        .WriteLine(strShipToAddress4, shippingLabelRptLineJustification.col7)
                    Else
                        .WriteLine(strShipToCityState, shippingLabelRptLineJustification.col7)
                        bolCityState = True
                    End If
                Else
                    .WriteLine()
                End If
                If bolCityState = False Then
                    If Not Trim(strShipToAddress4) = "" Then
                        .WriteLine(strShipToAddress4, shippingLabelRptLineJustification.col7)
                    Else
                        .WriteLine(strShipToCityState, shippingLabelRptLineJustification.col7)
                        bolCityState = True
                    End If
                End If
                If bolCityState = False Then
                    .WriteLine(strShipToCityState, shippingLabelRptLineJustification.col7)
                    bolCityState = True
                End If

            End If
            .CurrentF = arial12B
            .CurrentH = arial12B.Height
            .WriteLine()
            .Write("Order Number:", shippingLabelRptLineJustification.col6)
            .WriteLine(CStr(ds.Tables(0).Rows(mRow).Item("ORDER_NO")))
            .WriteLine()
            .Write("Shipping Container: ", shippingLabelRptLineJustification.col6)
            .Write(CStr(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID")))

            Dim s As String = "INVENTORY TRANSFER"
            If (s.Length > 0) Then
                If (s.Length > 55) Then
                    s = s.Substring(0, 51) & " ..."
                End If
                .WriteLine()
                .CurrentY = 500
                .CurrentF = arial10
                .CurrentH = arial10.Height
                .Write(s, shippingLabelRptLineJustification.col6)
                .CurrentF = arial12B
                .CurrentH = arial12B.Height
            End If

            strPrevCntrID = CStr(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID"))
            .CurrentY = intEndY
        End With


    End Sub

    Private Sub PrintFooter(ByVal e As shippingLabelPageEventArgs)

        Dim strShipToDescr As String
        Dim strShipToAddress1 As String
        Dim strShipToAddress2 As String
        Dim strShipToAddress3 As String
        Dim strShipToAddress4 As String
        Dim strShipToCityState As String
        Dim bolCityState As Boolean

        Dim ver As String = ""

        Try
            ver = "v" & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString
        Catch ex As Exception
        End Try
        If ds2.Tables(0).Rows.Count > 0 Then
            If mSrcBU = "I0206" Then 'gez 10/10/2012
                strShipToDescr = CStr(ds.Tables(0).Rows(0).Item("ISA_EMPLOYEE_NAME"))  'gez 10/10/2012
                strShipToAddress1 = "" 'gez 10/10/2012
                strShipToAddress2 = "" 'gez 10/10/2012
                strShipToAddress3 = "" 'gez 10/10/2012
                strShipToAddress4 = "" 'gez 10/10/2012r 
                strShipToCityState = "" 'gez 10/10/2012
            Else 'gez 10/10/2012
                strShipToDescr = CStr(ds2.Tables(0).Rows(0).Item("DESCR"))
                strShipToAddress1 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS1"))
                strShipToAddress2 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS2"))
                strShipToAddress3 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS3"))
                strShipToAddress4 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS4"))
                strShipToCityState = CStr(ds2.Tables(0).Rows(0).Item("CITY")) & ", " & _
                CStr(ds2.Tables(0).Rows(0).Item("STATE")) & " " & _
                CStr(ds2.Tables(0).Rows(0).Item("POSTAL"))

            End If 'gez 10/10/2012
        End If
        'If ds2.Tables(0).Rows.Count > 0 Then
        '    strShipToDescr = CStr(ds2.Tables(0).Rows(0).Item("DESCR"))
        '    strShipToAddress1 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS1"))
        '    strShipToAddress2 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS2"))
        '    strShipToAddress3 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS3"))
        '    strShipToAddress4 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS4"))
        '    strShipToCityState = CStr(ds2.Tables(0).Rows(0).Item("CITY")) & ", " & _
        '                         CStr(ds2.Tables(0).Rows(0).Item("STATE")) & " " & _
        '                         CStr(ds2.Tables(0).Rows(0).Item("POSTAL"))

        'End If
        If ds.Tables(0).Rows.Count > 0 Then


            With e
                .CurrentY = 600
                .Write("Order Number:", shippingLabelRptLineJustification.col6)
                .Write(CStr(ds.Tables(0).Rows(mRow - 1).Item("ORDER_NO")), shippingLabelRptLineJustification.col8)
                .WriteLine()
                .Write("Order Date:", shippingLabelRptLineJustification.col6)
                .Write(CStr(ds.Tables(0).Rows(mRow - 1).Item("ORDER_DATE")), shippingLabelRptLineJustification.col8)
                .WriteLine()
                .Write("Work Order:", shippingLabelRptLineJustification.col6)
                .Write(CStr(ds.Tables(0).Rows(mRow - 1).Item("ISA_WORK_ORDER_NO")), shippingLabelRptLineJustification.col8)
                .WriteLine()

                .Write("Shipping Container:", shippingLabelRptLineJustification.col6)
                If Not IsDBNull(ds.Tables(0).Rows(mRow - 1).Item("SHIP_CNTR_ID")) Then
                    .Write(CStr(ds.Tables(0).Rows(mRow - 1).Item("SHIP_CNTR_ID")), shippingLabelRptLineJustification.col8)
                    .WriteLine()
                Else
                    .WriteLine()
                End If

                .WriteLine()
                .Write("Deliver To...:1", shippingLabelRptLineJustification.col6)
                .Write(strShipToDescr, shippingLabelRptLineJustification.col7)
                .WriteLine()
                .Write(strShipToAddress1, shippingLabelRptLineJustification.col7)
                .WriteLine()
                '.Write("Work Order:", shippingLabelRptLineJustification.col1)
                '.Write(ds.Tables(0).Rows(mRow).Item("ISA_WORK_ORDER_NO"), shippingLabelRptLineJustification.col2)
                If Not Trim(strShipToAddress2) = "" Then
                    .Write(strShipToAddress2, shippingLabelRptLineJustification.col7)
                    .WriteLine()
                ElseIf Not Trim(strShipToAddress3) = "" Then
                    .Write(strShipToAddress3, shippingLabelRptLineJustification.col7)
                    .WriteLine()
                ElseIf Not Trim(strShipToAddress4) = "" Then
                    .Write(strShipToAddress4, shippingLabelRptLineJustification.col7)
                    .WriteLine()
                Else
                    .Write(strShipToCityState, shippingLabelRptLineJustification.col7)
                    .WriteLine()
                    bolCityState = True
                End If
                '.CurrentF = IDAutomationHC39M
                '.CurrentH = IDAutomationHC39M.Height

                '.Write("Shipping Container:", shippingLabelRptLineJustification.col1)
                '.Write(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID"), shippingLabelRptLineJustification.col2)
                '.CurrentF = arial10
                '.CurrentH = arial10.Height
                If bolCityState = False Then
                    If Not Trim(strShipToAddress3) = "" Then
                        .Write(strShipToAddress3, shippingLabelRptLineJustification.col7)
                        .WriteLine()
                    ElseIf Not Trim(strShipToAddress4) = "" Then
                        .Write(strShipToAddress4, shippingLabelRptLineJustification.col7)
                        .WriteLine()
                    Else
                        .Write(strShipToCityState, shippingLabelRptLineJustification.col7)
                        .WriteLine()
                        bolCityState = True
                    End If
                Else
                    .WriteLine()
                End If
                '.Write("Shipping Container:", shippingLabelRptLineJustification.col1)
                '.Write(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID"), shippingLabelRptLineJustification.col2)

                If bolCityState = False Then
                    If Not Trim(strShipToAddress4) = "" Then
                        .Write(strShipToAddress4, shippingLabelRptLineJustification.col7)
                        .WriteLine()
                    Else
                        .Write(strShipToCityState, shippingLabelRptLineJustification.col7)
                        .WriteLine()
                        bolCityState = True
                    End If
                End If
                If bolCityState = False Then
                    .Write(strShipToCityState, shippingLabelRptLineJustification.col7)
                    .WriteLine()
                    bolCityState = True
                End If
                .WriteLine()
                .CurrentF = arial8
                .CurrentH = arial8.Height
                .CurrentY = .MarginBounds.Bottom + 30
                .Write("* indicates item is in multiple packages.", shippingLabelRptLineJustification.col6)
                .Write(Now().ToString, shippingLabelRptLineJustification.Right)
                .WriteLine()
                .Write("*Please contact SDI Customer Service at 888-435-7550 for returns or questions.", shippingLabelRptLineJustification.Left)
                If ver.Length > 0 Then
                    .Write(ver & "     _", shippingLabelRptLineJustification.Right)
                End If
                .WriteLine()

                If Not IsDBNull(ds.Tables(0).Rows(mRow - 1).Item("SHIP_CNTR_ID")) Then
                    If strPrevCntrIDFooter <> CStr(ds.Tables(0).Rows(mRow - 1).Item("SHIP_CNTR_ID")) Then
                        PrintDocumentOnPrintPageFromDLL(e, CStr(ds.Tables(0).Rows(mRow - 1).Item("SHIP_CNTR_ID")))
                        strPrevCntrIDFooter = CStr(ds.Tables(0).Rows(mRow - 1).Item("SHIP_CNTR_ID"))
                    End If

                End If

            End With
        End If

    End Sub

    Private Sub buildDatasets()

        Dim rtn As String = "shippingLabel.buildDatasets"

        'Dim strSQLstring As String
        'strSQLstring = "SELECT A.ORDER_NO," & vbCrLf & _
        '            " A.ORDER_INT_LINE_NO, A.SCHED_LINE_NO," & vbCrLf & _
        '            " A.INV_ITEM_ID, B.SHIP_CNTR_ID, L.ISA_WORK_ORDER_NO," & vbCrLf & _
        '            " (E.DESCR254 ||' MFG: '||M.MFG_ID||'/'||M.MFG_ITM_ID) as DESCR254," & vbCrLf & _
        '            " TO_CHAR(A.ISA_PACK_DTTM,'YYYY-MM-DD') as pick_date," & vbCrLf & _
        '            " (SELECT SUM( I.QTY_PICKED)" & vbCrLf & _
        '            " FROM PS_ISA_PICKING_INT I" & vbCrLf & _
        '            " WHERE I.ORDER_NO = A.ORDER_NO" & vbCrLf & _
        '            " AND I.ORDER_INT_LINE_NO = A.ORDER_INT_LINE_NO) as QTY_PICKED, A.DEMAND_SOURCE," & vbCrLf & _
        '            " L.UNIT_OF_MEASURE, L.QTY_ORDERED," & vbCrLf & _
        '            " TO_CHAR(C.ADD_DTTM,'MM/DD/YY') as ORDER_DATE," & vbCrLf & _
        '            " L.ISA_EMPLOYEE_ID, G.ISA_EMPLOYEE_NAME" & vbCrLf & _
        '            " FROM PS_ISA_PICKING_CNT B, PS_ISA_PICKING_INT A," & vbCrLf & _
        '            " PS_ORD_LINE L, PS_ISA_ORD_INTFC_H C," & vbCrLf & _
        '            " PS_INV_ITEMS E, PS_ISA_EMPL_TBL G, PS_ITEM_MFG M" & vbCrLf & _
        '            " WHERE A.SOURCE_BUS_UNIT = '" & mSrcBU & "'" & vbCrLf & _
        '            " AND A.ORDER_NO = '" & mOrderNo & "'" & vbCrLf & _
        '            " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT(+)" & vbCrLf & _
        '            " AND A.DEMAND_SOURCE = B.DEMAND_SOURCE(+)" & vbCrLf & _
        '            " AND A.SOURCE_BUS_UNIT = B.SOURCE_BUS_UNIT(+)" & vbCrLf & _
        '            " AND A.ORDER_NO = B.ORDER_NO(+)" & vbCrLf & _
        '            " AND A.ORDER_INT_LINE_NO = B.ORDER_INT_LINE_NO(+)" & vbCrLf & _
        '            " AND A.SCHED_LINE_NO = B.SCHED_LINE_NO(+)" & vbCrLf & _
        '            " AND A.INV_ITEM_ID = B.INV_ITEM_ID(+)" & vbCrLf & _
        '            " AND A.DEMAND_LINE_NO = B.DEMAND_LINE_NO(+)" & vbCrLf & _
        '            " AND A.SEQ_NBR = B.SEQ_NBR(+)" & vbCrLf & _
        '            " AND A.RECEIVER_ID = B.RECEIVER_ID(+)" & vbCrLf & _
        '            " AND A.RECV_LN_NBR = B.RECV_LN_NBR(+)" & vbCrLf & _
        '            " AND NOT A.ENDDTTM IS NULL" & vbCrLf
        ''If Not mOverRide = "Y" Then
        ''    strSQLstring = strSQLstring & " AND A.SHIP_DTTM IS NULL" & vbCrLf
        ''End If
        'If Not m_bIsIgnoreShipDTTM Then
        '    ' if overriden, query will not consider SHIP_DTTM to be NULL
        '    strSQLstring = strSQLstring & " AND A.SHIP_DTTM IS NULL" & vbCrLf
        'End If
        'strSQLstring = strSQLstring & " AND A.QTY_PICKED > 0" & vbCrLf & _
        '            " AND A.ORDER_NO = L.ORDER_NO" & vbCrLf & _
        '            " AND A.ORDER_INT_LINE_NO = L.ORDER_INT_LINE_NO" & vbCrLf & _
        '            " AND A.ORDER_NO = C.ORDER_NO" & vbCrLf & _
        '            " AND E.INV_ITEM_ID = A.INV_ITEM_ID" & vbCrLf & _
        '            " AND E.EFFDT =" & vbCrLf & _
        '            " (SELECT MAX(E_ED.EFFDT) FROM PS_INV_ITEMS E_ED" & vbCrLf & _
        '            " WHERE E.SETID = E_ED.SETID" & vbCrLf & _
        '            " AND E.INV_ITEM_ID = E_ED.INV_ITEM_ID" & vbCrLf & _
        '            " AND E_ED.EFFDT <= SYSDATE)" & vbCrLf & _
        '            " AND L.BUSINESS_UNIT = G.BUSINESS_UNIT(+)" & vbCrLf & _
        '            " AND L.ISA_EMPLOYEE_ID = G.ISA_EMPLOYEE_ID(+)" & vbCrLf & _
        '            " AND E.SETID = M.SETID (+)" & vbCrLf & _
        '            " AND E.INV_ITEM_ID = M.INV_ITEM_ID (+)" & vbCrLf & _
        '            " AND M.PREFERRED_MFG (+) = 'Y'" & vbCrLf & _
        '            " GROUP BY A.ORDER_NO, A.ORDER_INT_LINE_NO, A.SCHED_LINE_NO," & vbCrLf & _
        '            " A.INV_ITEM_ID, B.SHIP_CNTR_ID, L.ISA_WORK_ORDER_NO," & vbCrLf & _
        '            " (E.DESCR254 ||' MFG: '||M.MFG_ID||'/'||M.MFG_ITM_ID)," & vbCrLf & _
        '            " TO_CHAR(A.ISA_PACK_DTTM,'YYYY-MM-DD'), A.DEMAND_SOURCE," & vbCrLf & _
        '            " L.UNIT_OF_MEASURE, L.QTY_ORDERED," & vbCrLf & _
        '            " C.ADD_DTTM, L.ISA_EMPLOYEE_ID, G.ISA_EMPLOYEE_NAME" & vbCrLf & _
        '            " UNION ALL" & vbCrLf & _
        '            " SELECT A.ORDER_NO," & vbCrLf & _
        '            " A.ORDER_INT_LINE_NO, A.SCHED_LINE_NO," & vbCrLf & _
        '            " A.INV_ITEM_ID, B.SHIP_CNTR_ID, L.ISA_WORK_ORDER_NO," & vbCrLf & _
        '            " (E.DESCR254 ||' MFG: '||M.MFG_ID||'/'||M.MFG_ITM_ID) as DESCR254," & vbCrLf & _
        '            " TO_CHAR(A.ISA_PACK_DTTM,'YYYY-MM-DD') as pick_date," & vbCrLf & _
        '            " (SELECT SUM( I.QTY_PICKED)" & vbCrLf & _
        '            " FROM PS_ISA_PICKING_INT I" & vbCrLf & _
        '            " WHERE I.ORDER_NO = A.ORDER_NO" & vbCrLf & _
        '            " AND I.ORDER_INT_LINE_NO = A.ORDER_INT_LINE_NO) as QTY_PICKED, A.DEMAND_SOURCE," & vbCrLf & _
        '            " L.UNIT_OF_MEASURE, L.QTY_ORDERED," & vbCrLf & _
        '            " TO_CHAR(C.ADD_DTTM,'MM/DD/YY') as ORDER_DATE," & vbCrLf & _
        '            " L.ISA_EMPLOYEE_ID, G.ISA_EMPLOYEE_NAME" & vbCrLf & _
        '            " FROM PS_ISA_PICKING_CNT B, PS_ISA_PICKING_INT A," & vbCrLf & _
        '            " PS_ORD_LINE L, PS_ISA_ORD_INTFC_H C," & vbCrLf & _
        '            " PS_INV_ITEMS E, PS_ISA_EMPL_TBL G, PS_ITEM_MFG M" & vbCrLf & _
        '            " WHERE A.SOURCE_BUS_UNIT = '" & mSrcBU & "'" & vbCrLf & _
        '            " AND A.ORDER_NO = '" & mOrderNo & "'" & vbCrLf & _
        '            " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT(+)" & vbCrLf & _
        '            " AND A.DEMAND_SOURCE = B.DEMAND_SOURCE(+)" & vbCrLf & _
        '            " AND A.SOURCE_BUS_UNIT = B.SOURCE_BUS_UNIT(+)" & vbCrLf & _
        '            " AND A.ORDER_NO = B.ORDER_NO(+)" & vbCrLf & _
        '            " AND A.ORDER_INT_LINE_NO = B.ORDER_INT_LINE_NO(+)" & vbCrLf & _
        '            " AND A.SCHED_LINE_NO = B.SCHED_LINE_NO(+)" & vbCrLf & _
        '            " AND A.INV_ITEM_ID = B.INV_ITEM_ID(+)" & vbCrLf & _
        '            " AND A.DEMAND_LINE_NO = B.DEMAND_LINE_NO(+)" & vbCrLf & _
        '            " AND A.SEQ_NBR = B.SEQ_NBR(+)" & vbCrLf & _
        '            " AND A.RECEIVER_ID = B.RECEIVER_ID(+)" & vbCrLf & _
        '            " AND A.RECV_LN_NBR = B.RECV_LN_NBR(+)" & vbCrLf & _
        '            " AND NOT A.ENDDTTM IS NULL" & vbCrLf
        ''If Not mOverRide = "Y" Then
        ''    strSQLstring = strSQLstring & " AND A.SHIP_DTTM IS NULL" & vbCrLf
        ''End If
        'If Not m_bIsIgnoreShipDTTM Then
        '    ' if overriden, query will not consider SHIP_DTTM to be NULL
        '    strSQLstring = strSQLstring & " AND A.SHIP_DTTM IS NULL" & vbCrLf
        'End If
        'strSQLstring = strSQLstring & " AND A.QTY_PICKED = 0" & vbCrLf & _
        '            " AND A.ORDER_NO = L.ORDER_NO" & vbCrLf & _
        '            " AND A.ORDER_INT_LINE_NO = L.ORDER_INT_LINE_NO" & vbCrLf & _
        '            " AND (SELECT COUNT(*)" & vbCrLf & _
        '            " FROM PS_ISA_PICKING_INT F" & vbCrLf & _
        '            " WHERE A.ORDER_NO = F.ORDER_NO" & vbCrLf & _
        '            " AND A.ORDER_INT_LINE_NO = F.ORDER_INT_LINE_NO" & vbCrLf & _
        '            " AND A.SCHED_LINE_NO = F.SCHED_LINE_NO) = 1" & vbCrLf & _
        '            " AND A.ORDER_NO = C.ORDER_NO" & vbCrLf & _
        '            " AND E.INV_ITEM_ID = A.INV_ITEM_ID" & vbCrLf & _
        '            " AND E.EFFDT =" & vbCrLf & _
        '            " (SELECT MAX(E_ED.EFFDT) FROM PS_INV_ITEMS E_ED" & vbCrLf & _
        '            " WHERE E.SETID = E_ED.SETID" & vbCrLf & _
        '            " AND E.INV_ITEM_ID = E_ED.INV_ITEM_ID" & vbCrLf & _
        '            " AND E_ED.EFFDT <= SYSDATE)" & vbCrLf & _
        '            " AND L.BUSINESS_UNIT = G.BUSINESS_UNIT(+)" & vbCrLf & _
        '            " AND L.ISA_EMPLOYEE_ID = G.ISA_EMPLOYEE_ID(+)" & vbCrLf & _
        '            " AND E.SETID = M.SETID (+)" & vbCrLf & _
        '            " AND E.INV_ITEM_ID = M.INV_ITEM_ID (+)" & vbCrLf & _
        '            " AND M.PREFERRED_MFG (+) = 'Y'" & vbCrLf & _
        '            " GROUP BY A.ORDER_NO, A.ORDER_INT_LINE_NO, A.SCHED_LINE_NO," & vbCrLf & _
        '            " A.INV_ITEM_ID, B.SHIP_CNTR_ID, L.ISA_WORK_ORDER_NO," & vbCrLf & _
        '            " (E.DESCR254 ||' MFG: '||M.MFG_ID||'/'||M.MFG_ITM_ID)," & vbCrLf & _
        '            " TO_CHAR(A.ISA_PACK_DTTM,'YYYY-MM-DD'), A.DEMAND_SOURCE," & vbCrLf & _
        '            " L.UNIT_OF_MEASURE, L.QTY_ORDERED," & vbCrLf & _
        '            " C.ADD_DTTM, L.ISA_EMPLOYEE_ID, G.ISA_EMPLOYEE_NAME" & vbCrLf & _
        '            " ORDER BY SHIP_CNTR_ID, ORDER_INT_LINE_NO,  SCHED_LINE_NO"

        '' A.DEMAND_LINE_NO, was removed from the group by
        '' and a.isa_user_defined_1 <> 'PROCESSED'" & vbCrLf & _

        Dim sql As SQLBuilder = Nothing

        If Not m_bIsIgnoreShipDTTM Then
            ' if overriden, query will not consider SHIP_DTTM to be NULL
            sql = New SQLBuilder(myCommon.LoadQuery(m_pathSQLs & "shippingLabel_InvTransfer_SELECT_ignoreShipDT.sql"))
        Else
            ' user regular - SHIP_DTTM should be NULL
            sql = New SQLBuilder(myCommon.LoadQuery(m_pathSQLs & "shippingLabel_InvTransfer_SELECT.sql"))
        End If
        sql.Parameters.Add(":KEY_SOURCE_BU", mSrcBU)
        sql.Parameters.Add(":KEY_ORDER_NO", mOrderNo)

        LogVerbose(msg:=rtn & "::CN string : " & Me.oraCNstring)
        LogVerbose(msg:=rtn & "::executing : " & sql.ToString)

        'Dim dbConn As String = _
        '"Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=" & mDB

        ''"data source=ineroth;initial catalog=pubs;integrated security=SSPI"
        'Dim da As New OleDb.OleDbDataAdapter(strSQLstring, dbConn)
        Dim da As New OleDb.OleDbDataAdapter(sql.ToString, Me.oraCNstring)
        da.Fill(ds)

        sql = Nothing

        'strSQLstring = "SELECT B.ISA_CUST_NOTES, B.SHIPTO_ID" & vbCrLf & _
        '     " FROM PS_ISA_ORD_INTFC_H A, PS_ISA_ORD_INTFC_L B" & vbCrLf & _
        '     " WHERE A.BUSINESS_UNIT_OM = '" & mSrcBU & "'" & vbCrLf & _
        '     " AND A.ORDER_NO = '" & mOrderNo & "'" & vbCrLf & _
        '     " AND A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
        '     " AND B.INV_ITEM_ID <> ' '" & vbCrLf & _
        '     " AND ROWNUM < 2"

        'sql = New SQLBuilder(myCommon.LoadQuery(m_pathSQLs & "getCustNote_InvTransfer_SELECT.sql"))
        sql = New SQLBuilder(myCommon.LoadQuery(m_pathSQLs & "getShipTo_InvTransfer_SELECT.sql"))
        sql.Parameters.Add(":KEY_SOURCE_BU", mSrcBU)
        sql.Parameters.Add(":KEY_ORDER_NO", mOrderNo)

        LogVerbose(msg:=rtn & "::CN string : " & Me.oraCNstring)
        LogVerbose(msg:=rtn & "::executing : " & sql.ToString)

        'Dim da1 As New OleDb.OleDbDataAdapter(strSQLstring, dbConn)
        Dim da1 As New OleDb.OleDbDataAdapter(sql.ToString, Me.oraCNstring)

        da1.Fill(ds1)
        If ds1.Tables(0).Rows.Count > 0 Then
            ds2 = getShipto(CStr(ds1.Tables(0).Rows(0).Item("SHIPTO_ID")))
        Else
            ds2 = getShipto(" ")
        End If

        sql = Nothing

    End Sub

    Private Sub DrawRectangleFloat(ByVal e As Printing.PrintPageEventArgs, ByVal column As Integer, ByVal line As Integer, ByVal width As Integer, ByVal Height As Integer)
        ' Create pen.
        Dim blackPen As New Pen(Color.Black, 3)
        e.Graphics.DrawRectangle(blackPen, column, line, width, Height)
    End Sub

    Private Sub PrintDocumentOnPrintPageFromDLL(ByVal e As PrintPageEventArgs, ByVal strCntrID As String)
        Dim grfx As System.Drawing.Graphics = e.Graphics
        Dim myImage As System.Drawing.Imaging.Metafile
        'Ensure that the Graphics object is printing in MM instead of the default inches
        grfx.PageUnit = GraphicsUnit.Millimeter
        grfx.PageScale = 1.0F
        grfx.DrawString("", Me.Font, Brushes.Black, 0, 0)

        Dim NewBarcode As New IDAutomation.LinearServerControl.LinearBarcode
        NewBarcode.SymbologyID = IDAutomation.LinearServerControl.LinearBarcode.Symbologies.Code128
        NewBarcode.BarHeightCM = "1.350"
        NewBarcode.CheckCharacter = True
        NewBarcode.CheckCharacterInText = False
        'NewBarcode.Code128Set = IDAutomation.Windows.Forms.LinearBarCode.Barcode.Code128CharacterSets.Auto
        NewBarcode.LeftMarginCM = "0.200"
        NewBarcode.NarrowToWideRatio = "2.0"
        NewBarcode.PostnetHeightShort = "0.1270"
        NewBarcode.PostnetHeightTall = "0.3226"
        NewBarcode.PostnetSpacing = "0.066"
        NewBarcode.ShowText = False
        NewBarcode.SuppSeparationCM = "0.350"
        'NewBarcode.SymbologyID = IDAutomation.Windows.Forms.LinearBarCode.Barcode.Symbologies.Code128
        NewBarcode.TextFontColor = System.Drawing.Color.Black
        NewBarcode.TextMarginCM = "0.100"
        NewBarcode.TopMarginCM = "0.200"
        NewBarcode.UPCESystem = "1"
        NewBarcode.XDimensionCM = "0.0260"
        NewBarcode.XDimensionMILS = "10.2362"

        NewBarcode.DataToEncode = strCntrID
        NewBarcode.SymbologyID = NewBarcode.Symbologies.Code128

        myImage = NewBarcode.Picture

        grfx.ResetTransform()
        grfx.DrawImage(myImage, 24, 85)
    End Sub

    Private Sub LogVerbose(ByVal msg As String)
        Me.Logger.WriteVerboseLog(msg)
    End Sub

    Private Sub LogInfo(ByVal msg As String)
        Me.Logger.WriteInformationLog(msg)
    End Sub

    Private Sub LogWarning(ByVal msg As String)
        Me.Logger.WriteWarningLog(msg)
    End Sub

    Private Sub LogError(ByVal msg As String)
        Me.Logger.WriteErrorLog(msg)
    End Sub

#End Region

End Class
