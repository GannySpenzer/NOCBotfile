Imports System.ComponentModel
Imports System.Reflection
Imports System.Drawing
Imports System.Drawing.Printing
Imports System.Web
Imports System.Text.RegularExpressions


Public Class shippingLabelRegPick

    Inherits PrintDocument
    Implements ISDiReport

    Private Const oraCN_default_provider As String = "Provider=OraOLEDB.Oracle.1;"
    Private Const oraCN_default_creden As String = "User ID=einternet;Password=einternet;"
    Private Const oraCN_default_DB As String = "Data Source=RPTG"  '  prod"

    Private Const CNTR_STATE_EXCLUDE As String = "'PROCESSED','ZERO PICK','LOADED','PRINTED'"

    Private m_bIsIgnoreShipDTTM As Boolean = False
    Private m_logger As SDI.ApplicationLogger.IApplicationLogger = New SDI.ApplicationLogger.noAppLogger
    Private m_cfgVersion As String = ""
    Private m_oraCNstring As String = "" & _
                                      oraCN_default_provider & _
                                      oraCN_default_creden & _
                                      oraCN_default_DB & _
                                      ""
    Private m_pathSQLs As String = ""
    Private m_executionPath As String = ""
    Private m_cntrTypes As ArrayList = Nothing
    Private m_partialPickExcludeState As String = ""


    ''This line was added to allow printing as a DLL
    'Protected WithEvents LinearBarcode1 As IDAutomation.LinearServerControl.LinearBarcode
    Private WithEvents linearBarcoder As IDAutomation.LinearServerControl.LinearBarcode = Nothing

    Public Event AssignedPrintedPickedOrderLines(ByVal sender As Object, ByVal e As EventArgs) Implements ISDiReport.AssignedPrintedPickedOrderLines

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

    Public Property PartialPickExcludeState() As String
        Get
            Return m_partialPickExcludeState
        End Get
        Set(ByVal Value As String)
            m_partialPickExcludeState = Value
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

    Private m_lines As PrintedPickedOrderLines = Nothing

    Public ReadOnly Property PrintedPickedOrderLines() As PrintedPickedOrderLines Implements ISDiReport.PrintedPickedOrderLines
        Get
            If (m_lines Is Nothing) Then
                m_lines = New PrintedPickedOrderLines
            End If
            Return m_lines
        End Get
    End Property

#Region " Event declarations "


    ''' <summary>
    ''' Raised once immediately before anything is printed to the report. The cursor is on the first line of the first page.
    ''' </summary>
    Public Event ReportBegin(ByVal sender As Object, _
      ByVal e As shippingLabelRegPickEventArgs)
    ''' <summary>
    ''' Raised for each page immediately before anything is printed to that page. The cursor is on the first line of the page.
    ''' </summary>
    Public Event PrintPageBegin(ByVal sender As Object, _
      ByVal e As shippingLabelRegPickEventArgs)
    ''' <summary>
    ''' Raised for each page immediately after the header for the page has been printed. The cursor is on the first line of the report body.
    ''' </summary>
    Public Event PrintPageBodyEnd(ByVal sender As Object, _
    ByVal e As shippingLabelRegPickEventArgs)
    ''' <summary>
    ''' Raised for each page after the footer has been printed. The cursor is past the end of the footer, typically into the bottom margin of the page.
    ''' </summary>
    Public Event PrintPageEnd(ByVal sender As Object, _
      ByVal e As shippingLabelRegPickEventArgs)
    ''' <summary>
    ''' Raised once at the very end of the report after all other printing is complete. The cursor is past the end of the footer on the last page, typically into the bottom margin of the page.
    ''' </summary>
    Public Event ReportEnd(ByVal sender As Object, _
      ByVal e As shippingLabelRegPickEventArgs)

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
    Private mOverRide As String
    Private mOrderNo As String
    Private mDB As String
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
    Dim ds As New DataSet
    Dim ds1 As New DataSet
    Dim ds2 As New DataSet
    Dim ds3 As New DataSet
    Dim arrItemIDC As New ArrayList
    Dim strPrevCntrID As String
    Dim strPrevCntrIDFooter As String

    Public Sub New()

        mFont = New Font("Arial", 10)
        mBrush = Brushes.Black
        FooterLines = 2
        MyBase.DefaultPageSettings.Margins.Top = 10
        MyBase.DefaultPageSettings.Margins.Left = 10
        MyBase.DefaultPageSettings.Margins.Right = 10
        MyBase.DefaultPageSettings.Landscape = True

        m_partialPickExcludeState = CNTR_STATE_EXCLUDE

    End Sub

    Public Sub New(ByVal srcbu As String, ByVal orderNo As String, ByVal strDB As String)

        mFont = New Font("Arial", 10)
        mBrush = Brushes.Black
        mSrcBU = srcbu

        mOrderNo = orderNo
        mDB = strDB
        MyBase.DefaultPageSettings.Margins.Top = 10
        MyBase.DefaultPageSettings.Margins.Left = 10
        MyBase.DefaultPageSettings.Margins.Right = 10
        MyBase.DefaultPageSettings.Landscape = True
        arrItemIDC = New ArrayList

        m_partialPickExcludeState = CNTR_STATE_EXCLUDE

    End Sub

    Public Sub New(ByVal srcbu As String, ByVal orderNo As String, ByVal strDB As String, ByVal strShipOverRide As String)

        mFont = New Font("Arial", 10)
        mBrush = Brushes.Black
        mSrcBU = srcbu
        mOverRide = strShipOverRide
        mOrderNo = orderNo
        mDB = strDB
        MyBase.DefaultPageSettings.Margins.Top = 10
        MyBase.DefaultPageSettings.Margins.Left = 10
        MyBase.DefaultPageSettings.Margins.Right = 10
        MyBase.DefaultPageSettings.Landscape = True
        arrItemIDC = New ArrayList

        m_partialPickExcludeState = CNTR_STATE_EXCLUDE

    End Sub

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
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        components = New System.ComponentModel.Container
    End Sub

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
        Try
            m_cntrTypes = containerType.GetContainerTypes(Me.ExecutionPath)
        Catch ex As Exception
        End Try

    End Sub

    Private Sub ReportDocument1_PrintPage(ByVal sender As Object, _
        ByVal e As System.Drawing.Printing.PrintPageEventArgs) Handles MyBase.PrintPage

        mPageNumber += 1
        mPageLine = 1

        ' create our ReportPageEventArgs1 object for this page
        Dim page As New shippingLabelRegPickEventArgs(e, mPageNumber, mFont, mBrush, mFooterLines)

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

    Private Sub PrintPageBodyStart(ByVal e As shippingLabelRegPickEventArgs)

        Dim strSQLstring As String
        Dim I As Integer
        Dim X As Integer
        Dim Z As Integer
        Dim arrNotes As ArrayList = New ArrayList
        Dim strFoot1 As String
        Dim strFoot2 As String
        Dim strFoot3 As String
        Dim strShipToDescr As String
        Dim strpreviousShipContainer As String
        Dim strpreviousLine As String

        Dim strcurrentShipContainer As String
        Dim intSavemY As Integer
        Dim intStartMY As Integer

        Dim strNotes As String = GetNotesForCurrentContainer()

        ' generate the header
        PrintHeader(e)

        With e
            If ds.Tables(0).Rows.Count = 0 Then
                .CurrentF = arial10B
                .WriteLine("No data found for this order number! - " & mOrderNo, shippingLblRegPickLineJustification.Left)
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
            .Write("Line-Sched", shippingLblRegPickLineJustification.colLineSched)
            .Write("Product Number", shippingLblRegPickLineJustification.colProductNumber)
            .Write("Ship Date", shippingLblRegPickLineJustification.colShipDate)
            .Write("UOM", shippingLblRegPickLineJustification.colUOM)
            .Write("Qty", shippingLblRegPickLineJustification.colQtyOrdered)
            .Write("Qty", shippingLblRegPickLineJustification.colQtyShipped)
            .WriteLine("Qty", shippingLblRegPickLineJustification.colQtyBackorder)

            .Write("Description", shippingLblRegPickLineJustification.colProductNumber)
            .Write("Ordered:", shippingLblRegPickLineJustification.colQtyOrdered)
            .Write("Shipped:", shippingLblRegPickLineJustification.colQtyShipped)
            .WriteLine("Backorder:", shippingLblRegPickLineJustification.colQtyBackorder)
            .HorizontalRule()

            ''arrNotes = FormatNotes(e, ds.Tables(0).Rows(I).Item("ISA_CUST_NOTES").toupper, arial8)
            'arrNotes = FormatWrap(e, CStr(ds1.Tables(0).Rows(0).Item("ISA_CUST_NOTES")).ToUpper, arial8, 640)
            arrNotes = FormatWrap(e, strNotes.ToUpper, arial8, 640)

            intSavemY = .CurrentY
            .CurrentY = .CurrentY2
            .CurrentF = arial10B
            .CurrentH = arial10B.Height
            .WriteLine("Delivery Notes:", shippingLblRegPickLineJustification.Left)
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

                    Dim nShadedLines As Integer = arrDescr.Count
                    Dim s1 As String = ""
                    Try
                        s1 = CStr(ds.Tables(0).Rows(I).Item("ISA_CUST_NOTES")).Trim
                    Catch ex As Exception
                    End Try
                    If (s1.Length > 0) Then
                        nShadedLines += 1
                    End If

                    If Not (mPageLine Mod 2) = 0 Then
                        .HorizontalShading(intStartMY, nShadedLines)
                    End If

                    strpreviousShipContainer = strcurrentShipContainer

                    .Write((CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")) & " - " & CStr(ds.Tables(0).Rows(I).Item("SCHED_LINE_NO"))), shippingLblRegPickLineJustification.colLineSched)
                    intIndex = arrItemIDB.IndexOf(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")) & CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")))
                    If intIndex > -1 Then
                        .Write("*")
                    End If
                    .Write(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")), shippingLblRegPickLineJustification.colProductNumber)
                    .Write(Convert.ToString(ds.Tables(0).Rows(I).Item("pick_date")), shippingLblRegPickLineJustification.colShipDate)
                    .Write(CStr(ds.Tables(0).Rows(I).Item("UNIT_OF_MEASURE")), shippingLblRegPickLineJustification.colUOM)
                    intIndex = arrItemIDC.IndexOf(CStr(ds.Tables(0).Rows(I).Item("INV_ITEM_ID")) & CStr(ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO")))
                    If intIndex = -1 Then
                        .Write(CStr(ds.Tables(0).Rows(I).Item("QTY_ORDERED")), shippingLblRegPickLineJustification.colQtyOrdered)
                        .Write(CStr(ds.Tables(0).Rows(I).Item("QTY_PICKED")), shippingLblRegPickLineJustification.colQtyShipped)
                        .WriteLine(CStr(CDbl(ds.Tables(0).Rows(I).Item("QTY_ORDERED")) - CDbl(ds.Tables(0).Rows(I).Item("QTY_PICKED"))), _
                                   shippingLblRegPickLineJustification.colQtyBackorder)
                    Else
                        .Write("--", shippingLblRegPickLineJustification.colQtyOrdered)
                        .Write("--", shippingLblRegPickLineJustification.colQtyShipped)
                        .WriteLine("--", shippingLblRegPickLineJustification.colQtyBackorder)
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
                    '    .WriteLine(Convert.ToString(ds.Tables(0).Rows(I).Item("DESCR254")).Substring(0, 60), shippingLblRegPickLineJustification.colProductNumber)
                    'Else
                    '    .WriteLine(ds.Tables(0).Rows(I).Item("DESCR254"), shippingLblRegPickLineJustification.colProductNumber)
                    'End If

                    For X = 0 To arrDescr.Count - 1
                        .WriteLine(CStr(arrDescr(X)), shippingLblRegPickLineJustification.colProductNumber)
                    Next

                    Dim s As String = ""
                    Try
                        s = CStr(ds.Tables(0).Rows(I).Item("ISA_CUST_NOTES")).Trim
                    Catch ex As Exception
                    End Try
                    If (s.Length > 0) Then
                        .WriteLine("** " & s, shippingLblRegPickLineJustification.colProductNumber)
                    End If

                    .WriteLine()
                    mRow += 1
                    mPageLine += 1
                Next

            End If

        End With


    End Sub


    Private Sub GeneratePage(ByVal e As shippingLabelRegPickEventArgs)

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

    Private Function FormatWrap(ByVal e As shippingLabelRegPickEventArgs, ByVal currentText As String, ByVal pFont As Font, ByVal fieldsize As Integer) As ArrayList

        Dim arrText As ArrayList
        arrText = New ArrayList

        Try
            Dim lengthOfText As Integer
            Dim maxLengthOfALine As Integer
            Dim startingPosition As Integer
            Dim endingPosition As Integer
            Dim lineLength As Integer
            Dim line As String
            Dim decDiv As Decimal
            Dim I As Integer

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
        Catch ex As Exception
            arrText.Add(currentText)
        End Try

        'FormatWrap = arrText
        Return (arrText)

    End Function

    Private Function getShipto(ByVal strShipto As String) As DataSet

        Dim strSQLstring As String = "SELECT A.DESCR, A.ADDRESS1, A.ADDRESS2," & vbCrLf & _
                    " A.ADDRESS3, A.ADDRESS4, A.CITY, A.STATE, A.POSTAL" & vbCrLf & _
                    " FROM PS_LOCATION_TBL A" & vbCrLf & _
                    " WHERE A.EFFDT =" & vbCrLf & _
                    " (SELECT MAX(A_ED.EFFDT) FROM PS_LOCATION_TBL A_ED" & vbCrLf & _
                    " WHERE A.SETID = A_ED.SETID" & vbCrLf & _
                    " AND A.LOCATION = A_ED.LOCATION" & vbCrLf & _
                    " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                    " AND A.LOCATION = '" & strShipto & "'" & vbCrLf & _
                    " AND A.EFF_STATUS = 'A'"

        'Dim dbConn As String = _
        '"Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=" & mDB

        ''"data source=ineroth;initial catalog=pubs;integrated security=SSPI"
        'Dim da As New OleDb.OleDbDataAdapter(strSQLstring, dbConn)
        Dim da As New OleDb.OleDbDataAdapter(strSQLstring, Me.oraCNstring)
        Dim dsAddr As New DataSet
        da.Fill(dsAddr)
        Return dsAddr

    End Function

    Private Function getTextLength(ByVal e As shippingLabelRegPickEventArgs, ByVal currentText As String, ByVal pFont As Font) As Integer

        getTextLength = CInt(e.Graphics.MeasureString(currentText, pFont).Width)

    End Function

    Private Sub PrintHeader(ByVal e As shippingLabelRegPickEventArgs)

        Dim strSQLstring As String
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

            strSQLstring = "SELECT DISTINCT B.SHIP_CNTR_ID" & vbCrLf & _
                        " FROM PS_ISA_PICKING_INT A, PS_ISA_PICKING_CNT B" & vbCrLf & _
                        " WHERE A.SOURCE_BUS_UNIT = '" & mSrcBU & "'" & vbCrLf & _
                        " AND A.ORDER_NO = '" & mOrderNo & "'" & vbCrLf & _
                        " AND NOT A.ENDDTTM IS NULL" & vbCrLf & _
                        " AND A.QTY_PICKED > 0" & vbCrLf & _
                        " AND A.BUSINESS_UNIT = B.BUSINESS_UNIT" & vbCrLf & _
                        " AND A.DEMAND_SOURCE = B.DEMAND_SOURCE" & vbCrLf & _
                        " AND A.SOURCE_BUS_UNIT = B.SOURCE_BUS_UNIT" & vbCrLf & _
                        " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
                        " AND A.ORDER_INT_LINE_NO = B.ORDER_INT_LINE_NO" & vbCrLf & _
                        " AND A.SCHED_LINE_NO = B.SCHED_LINE_NO" & vbCrLf & _
                        " AND A.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf & _
                        " AND A.DEMAND_LINE_NO = B.DEMAND_LINE_NO" & vbCrLf & _
                        " AND A.SEQ_NBR = B.SEQ_NBR" & vbCrLf & _
                        " AND A.RECEIVER_ID = B.RECEIVER_ID" & vbCrLf & _
                        " AND A.RECV_LN_NBR = B.RECV_LN_NBR" & vbCrLf
            If (Me.PartialPickExcludeState.Trim.Length > 0) Then
                strSQLstring = strSQLstring & " AND A.ISA_USER_DEFINED_1 NOT IN (" & Me.PartialPickExcludeState & ") " & vbCrLf
            End If
            If Not mOverRide = "Y" Then
                strSQLstring = strSQLstring & " AND A.SHIP_DTTM IS NULL" & vbCrLf
            End If

            'Dim da3 As New OleDb.OleDbDataAdapter(strSQLstring, dbConn)
            Dim da3 As New OleDb.OleDbDataAdapter(strSQLstring, Me.oraCNstring)

            da3.Fill(ds3)
            intContainers = ds3.Tables(0).Rows.Count

        End If

        With e

            .RotatePrint()

            If ds.Tables(0).Rows.Count = 0 Then
                .CurrentF = arial10B
                .WriteLine("No data found for this order number! - " & mOrderNo, shippingLblRegPickLineJustification.Centered)
                Exit Sub
            End If
            intStartY = e.CurrentY
            DrawRectangleFloat(e, 675, .CurrentY, 390, 510)
            .CurrentF = arial30B
            .CurrentH = arial30B.Height
            .Write("SDI, Inc. ", shippingLblRegPickLineJustification.Left)
            .CurrentF = arial20B
            .Write("Packing Slip", 425)
            .WriteLine()
            .CurrentF = arial8
            .CurrentH = arial8.Height
            .Write("The Power of Integrated Supply", shippingLblRegPickLineJustification.Left)
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
                'strShipToDescr = CStr(ds2.Tables(0).Rows(0).Item("DESCR"))
                'strShipToAddress1 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS1"))
                'strShipToAddress2 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS2"))
                'strShipToAddress3 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS3"))
                'strShipToAddress4 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS4"))
                'strShipToCityState = CStr(ds2.Tables(0).Rows(0).Item("CITY")) & ", " & _
                '                     CStr(ds2.Tables(0).Rows(0).Item("STATE")) & " " & _
                '                     CStr(ds2.Tables(0).Rows(0).Item("POSTAL"))
            Else
                If mSrcBU = "I0206" Then 'gez 10/10/2012
                    strShipToDescr = CStr(ds.Tables(0).Rows(0).Item("ISA_EMPLOYEE_NAME"))  'gez 10/10/2012
                End If

            End If

            .CurrentF = arial30B
            .CurrentH = arial30B.Height
            .Write("SDI, Inc. ", shippingLblRegPickLineJustification.col6)
            .WriteLine()
            .CurrentF = arial8
            .CurrentH = arial8.Height
            .Write("The Power of Integrated Supply", shippingLblRegPickLineJustification.col6)
            .WriteLine()
            .WriteLine()
            .CurrentF = arial20B
            .CurrentH = arial20B.Height

            If IsDBNull(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID")) Then
                .Write("Items are back ordered.", shippingLblRegPickLineJustification.col6)
                .WriteLine()
                .CurrentY = intEndY
                Exit Sub
            End If
            For I = 0 To ds3.Tables(0).Rows.Count - 1
                If CStr(ds3.Tables(0).Rows(I).Item("SHIP_CNTR_ID")) = CStr(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID")) Then
                    Exit For
                End If
            Next

            .Write("Package " & (I + 1) & " of " & ds3.Tables(0).Rows.Count, shippingLblRegPickLineJustification.col6)
            .WriteLine()
            If strPrevCntrID <> CStr(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID")) Then

                .CurrentH = arial8.Height
                .WriteLine()
                .CurrentH = arial20B.Height
                .Write(CStr(ds1.Tables(0).Rows(0).Item("SHIPTO_ID")), shippingLblRegPickLineJustification.col6)
                .WriteLine()
                .CurrentH = arial8.Height
                .WriteLine()
                .CurrentH = arial20B.Height
                .Write("Work Order:", shippingLblRegPickLineJustification.col6)
                .Write(CStr(ds.Tables(0).Rows(mRow).Item("ISA_WORK_ORDER_NO")))
                .WriteLine()
                .CurrentH = arial8.Height
                .WriteLine()
                .CurrentF = arial12B
                .CurrentH = arial12B.Height


                .Write("Deliver To:", shippingLblRegPickLineJustification.col6)
                If mSrcBU = "I0206" Then 'gez 10/10/2012
                    .WriteLine(strShipToDescr, shippingLblRegPickLineJustification.col7)
                Else
                    .WriteLine(strShipToAddress1, shippingLblRegPickLineJustification.col7)
                End If
                ' .WriteLine(strShipToAddress1, shippingLblRegPickLineJustification.col7)

                If Not Trim(strShipToAddress2) = "" Then
                    .WriteLine(strShipToAddress2, shippingLblRegPickLineJustification.col7)
                ElseIf Not Trim(strShipToAddress3) = "" Then
                    .WriteLine(strShipToAddress3, shippingLblRegPickLineJustification.col7)
                ElseIf Not Trim(strShipToAddress4) = "" Then
                    .WriteLine(strShipToAddress4, shippingLblRegPickLineJustification.col7)
                Else
                    .WriteLine(strShipToCityState, shippingLblRegPickLineJustification.col7)
                    bolCityState = True
                End If

                If bolCityState = False Then
                    If Not Trim(strShipToAddress3) = "" Then
                        .WriteLine(strShipToAddress3, shippingLblRegPickLineJustification.col7)
                    ElseIf Not Trim(strShipToAddress4) = "" Then
                        .WriteLine(strShipToAddress4, shippingLblRegPickLineJustification.col7)
                    Else
                        .WriteLine(strShipToCityState, shippingLblRegPickLineJustification.col7)
                        bolCityState = True
                    End If
                Else
                    .WriteLine()
                End If
                If bolCityState = False Then
                    If Not Trim(strShipToAddress4) = "" Then
                        .WriteLine(strShipToAddress4, shippingLblRegPickLineJustification.col7)
                    Else
                        .WriteLine(strShipToCityState, shippingLblRegPickLineJustification.col7)
                        bolCityState = True
                    End If
                End If
                If bolCityState = False Then
                    .WriteLine(strShipToCityState, shippingLblRegPickLineJustification.col7)
                    bolCityState = True
                End If

            End If
            .CurrentF = arial12B
            .CurrentH = arial12B.Height
            .WriteLine()
            .Write("Order Number:", shippingLblRegPickLineJustification.col6)
            .WriteLine(CStr(ds.Tables(0).Rows(mRow).Item("ORDER_NO")))
            .WriteLine()
            .Write("Shipping Container: ", shippingLblRegPickLineJustification.col6)
            .Write(CStr(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID")))
            .WriteLine()
            Dim containerType As String = ""
            Try
                containerType = GetContainerTypeName(CStr(ds.Tables(0).Rows(mRow).Item("CONTAINER_TYPE_ID")))
            Catch ex As Exception
            End Try
            .Write("Container Type: ", shippingLblRegPickLineJustification.col6)
            .Write(containerType)

            Dim s As String = GetNotesForCurrentContainer()
            If (s.Length > 0) Then
                If (s.Length > 55) Then
                    s = s.Substring(0, 51) & " ..."
                End If
                .WriteLine()
                .CurrentY = 500
                .CurrentF = arial8
                .CurrentH = arial8.Height
                .Write(s, shippingLblRegPickLineJustification.col6)
                .CurrentF = arial12B
                .CurrentH = arial12B.Height
            End If

            strPrevCntrID = CStr(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID"))
            .CurrentY = intEndY
        End With


    End Sub

    Private Sub PrintFooter(ByVal e As shippingLabelRegPickEventArgs)

        Dim strShipToDescr As String
        Dim strShipToAddress1 As String
        Dim strShipToAddress2 As String
        Dim strShipToAddress3 As String
        Dim strShipToAddress4 As String
        Dim strShipToCityState As String
        Dim bolCityState As Boolean

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
            'strShipToDescr = CStr(ds2.Tables(0).Rows(0).Item("DESCR"))
            'strShipToAddress1 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS1"))
            'strShipToAddress2 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS2"))
            'strShipToAddress3 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS3"))
            'strShipToAddress4 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS4"))
            'strShipToCityState = CStr(ds2.Tables(0).Rows(0).Item("CITY")) & ", " & _
            '                     CStr(ds2.Tables(0).Rows(0).Item("STATE")) & " " & _
            '                     CStr(ds2.Tables(0).Rows(0).Item("POSTAL"))
        Else
            If mSrcBU = "I0206" Then 'gez 10/10/2012
                strShipToDescr = CStr(ds.Tables(0).Rows(0).Item("ISA_EMPLOYEE_NAME"))  'gez 10/10/2012
            End If
            'strShipToDescr = CStr(ds.Tables(0).Rows(0).Item("ISA_EMPLOYEE_NAME"))  'gez 10/10/2012
        End If
        If ds.Tables(0).Rows.Count > 0 Then


            With e
                .CurrentY = 600
                .Write("Order Number:", shippingLblRegPickLineJustification.col6)
                .Write(CStr(ds.Tables(0).Rows(mRow - 1).Item("ORDER_NO")), shippingLblRegPickLineJustification.col8)
                .WriteLine()
                .Write("Order Date:", shippingLblRegPickLineJustification.col6)
                .Write(CStr(ds.Tables(0).Rows(mRow - 1).Item("ORDER_DATE")), shippingLblRegPickLineJustification.col8)
                .WriteLine()
                .Write("Work Order:", shippingLblRegPickLineJustification.col6)
                .Write(CStr(ds.Tables(0).Rows(mRow - 1).Item("ISA_WORK_ORDER_NO")), shippingLblRegPickLineJustification.col8)
                .WriteLine()

                .Write("Shipping Container:", shippingLblRegPickLineJustification.col6)
                If Not IsDBNull(ds.Tables(0).Rows(mRow - 1).Item("SHIP_CNTR_ID")) Then
                    .Write(CStr(ds.Tables(0).Rows(mRow - 1).Item("SHIP_CNTR_ID")), shippingLblRegPickLineJustification.col8)
                    .WriteLine()
                Else
                    .WriteLine()
                End If
                Dim containerType As String = ""
                Try
                    containerType = GetContainerTypeName(CStr(ds.Tables(0).Rows(mRow - 1).Item("CONTAINER_TYPE_ID")))
                Catch ex As Exception
                End Try
                .Write("Container Type: ", shippingLblRegPickLineJustification.col6)
                .Write(containerType, shippingLblRegPickLineJustification.col8)
                .WriteLine()

                .WriteLine()
                .Write("Deliver To: ", shippingLblRegPickLineJustification.col6)
                If mSrcBU = "I0206" Then 'gez 10/10/2012
                    .WriteLine(strShipToDescr, shippingLabelRptLineJustification.col7)
                Else
                    .WriteLine(strShipToAddress1, shippingLabelRptLineJustification.col7)
                End If
                ' .Write(strShipToDescr, shippingLblRegPickLineJustification.col7)
                .WriteLine()
                .Write(strShipToAddress1, shippingLblRegPickLineJustification.col7)
                .WriteLine()
                '.Write("Work Order:", shippingLblRegPickLineJustification.col1)
                '.Write(ds.Tables(0).Rows(mRow).Item("ISA_WORK_ORDER_NO"), shippingLblRegPickLineJustification.col2)
                If Not Trim(strShipToAddress2) = "" Then
                    .Write(strShipToAddress2, shippingLblRegPickLineJustification.col7)
                    .WriteLine()
                ElseIf Not Trim(strShipToAddress3) = "" Then
                    .Write(strShipToAddress3, shippingLblRegPickLineJustification.col7)
                    .WriteLine()
                ElseIf Not Trim(strShipToAddress4) = "" Then
                    .Write(strShipToAddress4, shippingLblRegPickLineJustification.col7)
                    .WriteLine()
                Else
                    .Write(strShipToCityState, shippingLblRegPickLineJustification.col7)
                    .WriteLine()
                    bolCityState = True
                End If
                '.CurrentF = IDAutomationHC39M
                '.CurrentH = IDAutomationHC39M.Height

                '.Write("Shipping Container:", shippingLblRegPickLineJustification.col1)
                '.Write(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID"), shippingLblRegPickLineJustification.col2)
                '.CurrentF = arial10
                '.CurrentH = arial10.Height
                If bolCityState = False Then
                    If Not Trim(strShipToAddress3) = "" Then
                        .Write(strShipToAddress3, shippingLblRegPickLineJustification.col7)
                        .WriteLine()
                    ElseIf Not Trim(strShipToAddress4) = "" Then
                        .Write(strShipToAddress4, shippingLblRegPickLineJustification.col7)
                        .WriteLine()
                    Else
                        .Write(strShipToCityState, shippingLblRegPickLineJustification.col7)
                        .WriteLine()
                        bolCityState = True
                    End If
                Else
                    .WriteLine()
                End If
                '.Write("Shipping Container:", shippingLblRegPickLineJustification.col1)
                '.Write(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID"), shippingLblRegPickLineJustification.col2)

                If bolCityState = False Then
                    If Not Trim(strShipToAddress4) = "" Then
                        .Write(strShipToAddress4, shippingLblRegPickLineJustification.col7)
                        .WriteLine()
                    Else
                        .Write(strShipToCityState, shippingLblRegPickLineJustification.col7)
                        .WriteLine()
                        bolCityState = True
                    End If
                End If
                If bolCityState = False Then
                    .Write(strShipToCityState, shippingLblRegPickLineJustification.col7)
                    .WriteLine()
                    bolCityState = True
                End If
                .WriteLine()
                .CurrentF = arial8
                .CurrentH = arial8.Height
                .CurrentY = .MarginBounds.Bottom + 30
                .Write("* indicates item is in multiple packages.", shippingLblRegPickLineJustification.col6)
                .Write(Now().ToString, shippingLblRegPickLineJustification.Right)
                .WriteLine()
                .Write("*Please contact SDI Customer Service at 888-435-7550 for returns or questions.", shippingLblRegPickLineJustification.Left)
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

        Dim rtn As String = "shippingLabelRegPick.buildDatasets"

        Dim bu3 As String = "" 'mSrcBU
        Try
            bu3 = mSrcBU.Trim.ToUpper.Substring(2, 3)
        Catch ex As Exception
            bu3 = "~" 'to prevent from grabbing any B/U
        End Try

        Dim strSQLstring As String = ""
        strSQLstring = "" & _
                        "SELECT " & vbCrLf & _
                        "  VW.ORDER_NO  " & vbCrLf & _
                        ", VW.ORDER_INT_LINE_NO  " & vbCrLf & _
                        ", VW.SCHED_LINE_NO  " & vbCrLf & _
                        ", VW.INV_ITEM_ID  " & vbCrLf & _
                        ", VW.SHIP_CNTR_ID  " & vbCrLf & _
                        ", VW.ISA_WORK_ORDER_NO  " & vbCrLf & _
                        ", VW.DESCR254  " & vbCrLf & _
                        ", VW.PICK_DATE  " & vbCrLf & _
                        ", SUM (VW.QTY_PICKED) AS QTY_PICKED " & vbCrLf & _
                        ", VW.DEMAND_SOURCE  " & vbCrLf & _
                        ", VW.UNIT_OF_MEASURE  " & vbCrLf & _
                        ", VW.QTY_ORDERED  " & vbCrLf & _
                        ", VW.ORDER_DATE  " & vbCrLf & _
                        ", VW.ISA_EMPLOYEE_ID  " & vbCrLf & _
                        ", VW.ISA_EMPLOYEE_NAME  " & vbCrLf & _
                        ", VW.CONTAINER_TYPE_ID  " & vbCrLf & _
                        ", VW.ISA_CUST_NOTES " & vbCrLf & _
                        "FROM ( " & vbCrLf & _
                        "     SELECT DISTINCT  " & vbCrLf & _
                        "       A.ORDER_NO " & vbCrLf & _
                        "     , A.ORDER_INT_LINE_NO " & vbCrLf & _
                        "     , A.SCHED_LINE_NO " & vbCrLf & _
                        "     , A.INV_ITEM_ID " & vbCrLf & _
                        "     , B.SHIP_CNTR_ID " & vbCrLf & _
                        "     , L.ISA_WORK_ORDER_NO " & vbCrLf & _
                        "     , (E.DESCR254 ||' MFG: '||M.MFG_ID||'/'||M.MFG_ITM_ID) AS DESCR254 " & vbCrLf & _
                        "     , TO_CHAR(A.ISA_PACK_DTTM,'YYYY-MM-DD') AS PICK_DATE " & vbCrLf & _
                        "     , ( " & vbCrLf & _
                        "		  SELECT SUM (A1.QTY_PICKED) FROM SYSADM.PS_ISA_PICKING_INT A1 " & vbCrLf & _
                        "		  WHERE A1.ORDER_NO = A.ORDER_NO " & vbCrLf & _
                        "		    AND A1.ORDER_INT_LINE_NO = A.ORDER_INT_LINE_NO " & vbCrLf & _
                        "		    AND A1.INV_ITEM_ID = A.INV_ITEM_ID " & vbCrLf & _
                        "       ) AS QTY_PICKED " & vbCrLf & _
                        "     , A.DEMAND_SOURCE " & vbCrLf & _
                        "     , L.UNIT_OF_MEASURE " & vbCrLf & _
                        "     , L.QTY_ORDERED " & vbCrLf & _
                        "     , TO_CHAR(C.ADD_DTTM,'MM/DD/YY') AS ORDER_DATE " & vbCrLf & _
                        "     , L.ISA_EMPLOYEE_ID " & vbCrLf & _
                        "     , G.ISA_EMPLOYEE_NAME " & vbCrLf & _
                        "     , B.CONTAINER_TYPE AS CONTAINER_TYPE_ID " & vbCrLf & _
                        "     , X.ISA_CUST_NOTES " & vbCrLf & _
                        "     FROM SYSADM.PS_ISA_PICKING_INT A " & vbCrLf & _
                        "     INNER JOIN SYSADM.PS_ORD_LINE L " & vbCrLf & _
                        "       ON L.ORDER_NO          = A.REFERENCE_ID " & vbCrLf & _
                        "      AND L.ORDER_INT_LINE_NO = A.ORDER_INT_LINE_NO " & vbCrLf & _
                        "     LEFT OUTER JOIN SYSADM.PS_ISA_ORD_INTFC_H C " & vbCrLf & _
                        "       ON C.ORDER_NO          = A.REFERENCE_ID " & vbCrLf & _
                        "     LEFT OUTER JOIN SYSADM.PS_ISA_ORD_INTFC_L X " & vbCrLf & _
                        "       ON X.ISA_PARENT_IDENT  = C.ISA_IDENTIFIER " & vbCrLf & _
                        "      AND X.LINE_NBR          = A.ORDER_INT_LINE_NO " & vbCrLf & _
                        "     LEFT OUTER JOIN SYSADM.PS_ISA_PICKING_CNT B " & vbCrLf & _
                        "       ON B.BUSINESS_UNIT     = A.BUSINESS_UNIT " & vbCrLf & _
                        "      AND B.DEMAND_SOURCE     = A.DEMAND_SOURCE " & vbCrLf & _
                        "      AND B.SOURCE_BUS_UNIT   = A.SOURCE_BUS_UNIT  " & vbCrLf & _
                        "      AND B.ORDER_NO          = A.ORDER_NO  " & vbCrLf & _
                        "      AND B.ORDER_INT_LINE_NO = A.ORDER_INT_LINE_NO  " & vbCrLf & _
                        "      AND B.SCHED_LINE_NO     = A.SCHED_LINE_NO  " & vbCrLf & _
                        "      AND B.INV_ITEM_ID       = A.INV_ITEM_ID  " & vbCrLf & _
                        "      AND B.DEMAND_LINE_NO    = A.DEMAND_LINE_NO  " & vbCrLf & _
                        "      AND B.SEQ_NBR           = A.SEQ_NBR  " & vbCrLf & _
                        "      AND B.RECEIVER_ID       = A.RECEIVER_ID  " & vbCrLf & _
                        "      AND B.RECV_LN_NBR       = A.RECV_LN_NBR  " & vbCrLf & _
                        "     LEFT OUTER JOIN SYSADM.PS_INV_ITEMS E " & vbCrLf & _
                        "       ON E.SETID             = 'MAIN1' " & vbCrLf & _
                        "      AND E.INV_ITEM_ID       = A.INV_ITEM_ID " & vbCrLf & _
                        "     LEFT OUTER JOIN SYSADM.PS_ISA_EMPL_TBL G " & vbCrLf & _
                        "       ON G.BUSINESS_UNIT     = C.BUSINESS_UNIT_OM " & vbCrLf & _
                        "      AND G.ISA_EMPLOYEE_ID   = L.ISA_EMPLOYEE_ID  " & vbCrLf & _
                        "     LEFT OUTER JOIN SYSADM.PS_ITEM_MFG M " & vbCrLf & _
                        "       ON M.SETID             = E.SETID  " & vbCrLf & _
                        "      AND M.INV_ITEM_ID       = E.INV_ITEM_ID " & vbCrLf & _
                        "      AND M.PREFERRED_MFG     = 'Y' " & vbCrLf & _
                        "     WHERE  " & vbCrLf & _
                        "         A.SOURCE_BUS_UNIT LIKE '%" & bu3 & "' " & vbCrLf & _
                        "     AND A.ORDER_NO = '" & mOrderNo & "' " & vbCrLf & _
                        "     AND A.DEMAND_SOURCE IN ('OM','IN') " & vbCrLf & _
                        "     AND A.ENDDTTM IS NOT NULL " & vbCrLf & _
                        ""
        If (Me.PartialPickExcludeState.Trim.Length > 0) Then
            strSQLstring &= "     AND A.ISA_USER_DEFINED_1 NOT IN (" & Me.PartialPickExcludeState & ") " & vbCrLf
        End If
        If Not mOverRide = "Y" Then
            strSQLstring &= "     AND A.SHIP_DTTM IS NULL " & vbCrLf
        End If
        strSQLstring &= "" & _
                        "     AND E.EFFDT = ( " & vbCrLf & _
                        "                       SELECT MAX(E_ED.EFFDT) FROM SYSADM.PS_INV_ITEMS E_ED " & vbCrLf & _
                        "                       WHERE E.SETID = E_ED.SETID " & vbCrLf & _
                        "                       AND E.INV_ITEM_ID = E_ED.INV_ITEM_ID " & vbCrLf & _
                        "                       AND E_ED.EFFDT <= SYSDATE " & vbCrLf & _
                        "                   ) " & vbCrLf & _
                        "    UNION " & vbCrLf & _
                        "    SELECT DISTINCT  " & vbCrLf & _
                        "      A.ORDER_NO  " & vbCrLf & _
                        "    , A.ORDER_INT_LINE_NO  " & vbCrLf & _
                        "    , A.SCHED_LINE_NO  " & vbCrLf & _
                        "    , A.INV_ITEM_ID  " & vbCrLf & _
                        "    , B.SHIP_CNTR_ID  " & vbCrLf & _
                        "    , REQ.ISA_WORK_ORDER_NO  " & vbCrLf & _
                        "    , (REQ.DESCR254_MIXED ||' MFG: '||REQ.MFG_ID||'/'||REQ.MFG_ITM_ID) AS DESCR254  " & vbCrLf & _
                        "    , TO_CHAR(A.ISA_PACK_DTTM,'YYYY-MM-DD') AS PICK_DATE  " & vbCrLf & _
                        "    , ( " & vbCrLf & _
                        "		  SELECT SUM (A1.QTY_PICKED) FROM SYSADM.PS_ISA_PICKING_INT A1 " & vbCrLf & _
                        "		  WHERE A1.ORDER_NO = A.ORDER_NO " & vbCrLf & _
                        "		    AND A1.ORDER_INT_LINE_NO = A.ORDER_INT_LINE_NO " & vbCrLf & _
                        "		    AND A1.INV_ITEM_ID = A.INV_ITEM_ID " & vbCrLf & _
                        "      ) AS QTY_PICKED " & vbCrLf & _
                        "    , A.DEMAND_SOURCE " & vbCrLf & _
                        "    , REQ.UNIT_OF_MEASURE " & vbCrLf & _
                        "    , REQ.QTY_REQ AS QTY_ORDERED " & vbCrLf & _
                        "    , TO_CHAR(C.ADD_DTTM,'MM/DD/YY') AS ORDER_DATE " & vbCrLf & _
                        "    , REQ.ISA_EMPLOYEE_ID " & vbCrLf & _
                        "    , G.ISA_EMPLOYEE_NAME " & vbCrLf & _
                        "    , B.CONTAINER_TYPE AS CONTAINER_TYPE_ID " & vbCrLf & _
                        "    , X.ISA_CUST_NOTES " & vbCrLf & _
                        "    FROM SYSADM.PS_ISA_PICKING_INT A " & vbCrLf & _
                        "    INNER JOIN SYSADM.PS_REQ_LINE REQ " & vbCrLf & _
                        "       ON REQ.REQ_ID          = A.REFERENCE_ID  " & vbCrLf & _
                        "      AND REQ.LINE_NBR        = A.ORDER_INT_LINE_NO " & vbCrLf & _
                        "    LEFT OUTER JOIN SYSADM.PS_ISA_PICKING_CNT B " & vbCrLf & _
                        "      ON B.BUSINESS_UNIT      = A.BUSINESS_UNIT " & vbCrLf & _
                        "     AND B.DEMAND_SOURCE      = A.DEMAND_SOURCE  " & vbCrLf & _
                        "     AND B.SOURCE_BUS_UNIT    = A.SOURCE_BUS_UNIT  " & vbCrLf & _
                        "     AND B.ORDER_NO           = A.ORDER_NO  " & vbCrLf & _
                        "     AND B.ORDER_INT_LINE_NO  = A.ORDER_INT_LINE_NO  " & vbCrLf & _
                        "     AND B.SCHED_LINE_NO      = A.SCHED_LINE_NO  " & vbCrLf & _
                        "     AND B.INV_ITEM_ID        = A.INV_ITEM_ID  " & vbCrLf & _
                        "     AND B.DEMAND_LINE_NO     = A.DEMAND_LINE_NO  " & vbCrLf & _
                        "     AND B.SEQ_NBR            = A.SEQ_NBR  " & vbCrLf & _
                        "     AND B.RECEIVER_ID        = A.RECEIVER_ID  " & vbCrLf & _
                        "     AND B.RECV_LN_NBR        = A.RECV_LN_NBR  " & vbCrLf & _
                        "    LEFT OUTER JOIN SYSADM.PS_ISA_ORD_INTFC_H C " & vbCrLf & _
                        "      ON C.ORDER_NO           = A.REFERENCE_ID " & vbCrLf & _
                        "    LEFT OUTER JOIN SYSADM.PS_ISA_ORD_INTFC_L X " & vbCrLf & _
                        "      ON X.ISA_PARENT_IDENT   = C.ISA_IDENTIFIER " & vbCrLf & _
                        "     AND X.LINE_NBR           = A.ORDER_INT_LINE_NO " & vbCrLf & _
                        "    LEFT OUTER JOIN SYSADM.PS_ISA_EMPL_TBL G " & vbCrLf & _
                        "      ON G.BUSINESS_UNIT      = C.BUSINESS_UNIT_OM " & vbCrLf & _
                        "     AND G.ISA_EMPLOYEE_ID    = REQ.ISA_EMPLOYEE_ID " & vbCrLf & _
                        "    WHERE  " & vbCrLf & _
                        "         A.SOURCE_BUS_UNIT LIKE '%" & bu3 & "' " & vbCrLf & _
                        "     AND A.ORDER_NO = '" & mOrderNo & "' " & vbCrLf & _
                        "     AND A.DEMAND_SOURCE = 'OM' " & vbCrLf & _
                        "     AND A.ENDDTTM IS NOT NULL " & vbCrLf & _
                        ""
        If (Me.PartialPickExcludeState.Trim.Length > 0) Then
            strSQLstring &= "     AND A.ISA_USER_DEFINED_1 NOT IN (" & Me.PartialPickExcludeState & ") " & vbCrLf
        End If
        If Not mOverRide = "Y" Then
            strSQLstring &= "     AND A.SHIP_DTTM IS NULL " & vbCrLf
        End If
        strSQLstring &= "" & _
                        ") VW " & vbCrLf & _
                        "GROUP BY  " & vbCrLf & _
                        "  VW.ORDER_NO  " & vbCrLf & _
                        ", VW.ORDER_INT_LINE_NO  " & vbCrLf & _
                        ", VW.SCHED_LINE_NO  " & vbCrLf & _
                        ", VW.INV_ITEM_ID  " & vbCrLf & _
                        ", VW.SHIP_CNTR_ID  " & vbCrLf & _
                        ", VW.ISA_WORK_ORDER_NO  " & vbCrLf & _
                        ", VW.DESCR254  " & vbCrLf & _
                        ", VW.PICK_DATE  " & vbCrLf & _
                        ", VW.DEMAND_SOURCE  " & vbCrLf & _
                        ", VW.UNIT_OF_MEASURE  " & vbCrLf & _
                        ", VW.QTY_ORDERED  " & vbCrLf & _
                        ", VW.ORDER_DATE  " & vbCrLf & _
                        ", VW.ISA_EMPLOYEE_ID  " & vbCrLf & _
                        ", VW.ISA_EMPLOYEE_NAME  " & vbCrLf & _
                        ", VW.CONTAINER_TYPE_ID  " & vbCrLf & _
                        ", VW.ISA_CUST_NOTES " & vbCrLf & _
                        "ORDER BY  " & vbCrLf & _
                        "  VW.SHIP_CNTR_ID " & vbCrLf & _
                        ", VW.ORDER_INT_LINE_NO " & vbCrLf & _
                        ", VW.SCHED_LINE_NO " & vbCrLf & _
                        ""

        Dim da As New OleDb.OleDbDataAdapter(strSQLstring, Me.oraCNstring)
        da.Fill(ds)

        ' fill the "printed picked order lines" and raise the event
        FillPrintedPickedOrderLinesFromDS(ds)

        strSQLstring = "" & _
             "SELECT B.ISA_CUST_NOTES, B.SHIPTO_ID " & vbCrLf & _
             "FROM PS_ISA_ORD_INTFC_H A, PS_ISA_ORD_INTFC_L B " & vbCrLf & _
             "WHERE A.BUSINESS_UNIT_OM = '" & mSrcBU & "' " & vbCrLf & _
             "  AND A.ORDER_NO = '" & mOrderNo & "'" & vbCrLf & _
             "  AND A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
             "  AND B.INV_ITEM_ID <> ' '" & vbCrLf & _
             "  AND ROWNUM < 2" & vbCrLf & _
             ""

        ''Dim da1 As New OleDb.OleDbDataAdapter(strSQLstring, dbConn)
        'Dim da1 As New OleDb.OleDbDataAdapter(strSQLstring, Me.oraCNstring)
        Dim da1 As OleDb.OleDbDataAdapter = Nothing

        da1 = New OleDb.OleDbDataAdapter(strSQLstring, Me.oraCNstring)
        da1.Fill(ds1)
        If ds1.Tables(0).Rows.Count > 0 Then
            ds2 = getShipto(CStr(ds1.Tables(0).Rows(0).Item("SHIPTO_ID")))
        Else
            'ds2 = getShipto(" ")
            strSQLstring = "" & _
                 "SELECT B.ISA_CUST_NOTES, B.SHIPTO_ID " & vbCrLf & _
                 "FROM PS_ISA_ORD_INTFC_H A, PS_ISA_ORD_INTFC_L B " & vbCrLf & _
                 "WHERE A.BUSINESS_UNIT_OM = '" & mSrcBU & "' " & vbCrLf & _
                 "  AND A.ORDER_NO = '" & mOrderNo & "'" & vbCrLf & _
                 "  AND A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
                 "  AND B.INV_ITEM_ID = ' '" & vbCrLf & _
                 "  AND ROWNUM < 2" & vbCrLf & _
                 ""
            da1 = New OleDb.OleDbDataAdapter(strSQLstring, Me.oraCNstring)
            da1.Fill(ds1)
            If ds1.Tables(0).Rows.Count > 0 Then
                ds2 = getShipto(CStr(ds1.Tables(0).Rows(0).Item("SHIPTO_ID")))
            Else
                ds2 = getShipto(" ")
            End If
        End If

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

#End Region

    '// returns the full name for the container type based on the supplied Id
    '//     if Id not found, just return back the Id as the name
    Private Function GetContainerTypeName(ByVal sId As String) As String
        Dim sName As String = sId
        Try
            If Not (m_cntrTypes Is Nothing) Then
                For Each typ As containerType In m_cntrTypes
                    If typ.Id = sId Then
                        sName = typ.Description
                        Exit For
                    End If
                Next
            End If
        Catch ex As Exception
        End Try
        Return sName
    End Function

    Private Sub FillPrintedPickedOrderLinesFromDS(ByVal ds As DataSet)
        Try
            ' fill
            If Not (ds Is Nothing) Then
                If (ds.Tables.Count > 0) Then
                    If (ds.Tables(0).Rows.Count > 0) Then

                        Me.PrintedPickedOrderLines.Items.Clear()

                        Dim lne As PrintedPickedOrderLine = Nothing

                        For Each row As DataRow In ds.Tables(0).Rows
                            lne = New PrintedPickedOrderLine

                            Try
                                lne.OrderNo = CStr(row("ORDER_NO"))
                            Catch ex As Exception
                            End Try
                            If (lne.OrderNo.Length = 0) Then
                                lne.OrderNo = " "
                            End If
                            Try
                                lne.OrderLineNo = CInt(row("ORDER_INT_LINE_NO"))
                            Catch ex As Exception
                            End Try
                            Try
                                lne.SchedLineNo = CInt(row("SCHED_LINE_NO"))
                            Catch ex As Exception
                            End Try
                            Try
                                lne.InvItemId = CStr(row("INV_ITEM_ID"))
                            Catch ex As Exception
                            End Try
                            If (lne.InvItemId.Length = 0) Then
                                lne.InvItemId = " "
                            End If
                            Try
                                lne.DemandSource = CStr(row("DEMAND_SOURCE"))
                            Catch ex As Exception
                            End Try
                            If (lne.DemandSource.Length = 0) Then
                                lne.DemandSource = " "
                            End If
                            Try
                                lne.ContainerId = CStr(row("SHIP_CNTR_ID"))
                            Catch ex As Exception
                            End Try
                            If (lne.ContainerId.Length = 0) Then
                                lne.ContainerId = " "
                            End If

                            Me.PrintedPickedOrderLines.Items.Add(lne)
                        Next

                    End If
                End If
            End If
            ' raise event
            RaiseEvent AssignedPrintedPickedOrderLines(Me, Nothing)
        Catch ex As Exception
        End Try
    End Sub

    Private Function GetNotesForCurrentContainer() As String
        Dim strNotes As String = ""

        ' base (our current) container ID being printed
        Dim sCurrentContainerId As String = ""
        Try
            sCurrentContainerId = CStr(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID")).Trim.ToUpper
        Catch ex As Exception
        End Try

        If (ds.Tables(0).Rows.Count > 0) And (sCurrentContainerId.Length > 0) Then
            Dim sep1 As String = ";"
            Dim sNote As String = ""
            Dim sCntrId As String = ""
            For Each row As DataRow In ds.Tables(0).Rows
                sCntrId = ""
                Try
                    sCntrId = CStr(row("SHIP_CNTR_ID")).Trim.ToUpper
                Catch ex As Exception
                End Try
                sNote = ""
                Try
                    sNote = CStr(row("ISA_CUST_NOTES")).Trim.ToUpper
                Catch ex As Exception
                End Try
                If (sNote.Length > 0) And (sCurrentContainerId = sCntrId) Then
                    If (strNotes.Length = 0) Then
                        ' if I don't have any note yet
                        strNotes &= sNote & sep1
                    ElseIf (strNotes.IndexOf(sNote) = -1) Then
                        ' if I didn't find this specific note on my string
                        strNotes &= sNote & sep1
                    End If
                End If
            Next
            strNotes = strNotes.TrimEnd(sep1)
        End If

        Return (strNotes)
    End Function

End Class
