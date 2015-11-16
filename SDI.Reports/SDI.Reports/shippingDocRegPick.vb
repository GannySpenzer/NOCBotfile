Imports System.Drawing
Imports System.Drawing.Printing
Imports System.Text.RegularExpressions
Imports System.Xml


Public Class shippingDocRegPick

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
      ByVal e As shippingDocRegPickEventArgs)
    ''' <summary>
    ''' Raised for each page immediately before anything is printed to that page. The cursor is on the first line of the page.
    ''' </summary>
    Public Event PrintPageBegin(ByVal sender As Object, _
      ByVal e As shippingDocRegPickEventArgs)
    ''' <summary>
    ''' Raised for each page immediately after the header for the page has been printed. The cursor is on the first line of the report body.
    ''' </summary>
    Public Event PrintPageBodyEnd(ByVal sender As Object, _
    ByVal e As shippingDocRegPickEventArgs)
    ''' <summary>
    ''' Raised for each page after the footer has been printed. The cursor is past the end of the footer, typically into the bottom margin of the page.
    ''' </summary>
    Public Event PrintPageEnd(ByVal sender As Object, _
      ByVal e As shippingDocRegPickEventArgs)
    ''' <summary>
    ''' Raised once at the very end of the report after all other printing is complete. The cursor is past the end of the footer on the last page, typically into the bottom margin of the page.
    ''' </summary>
    Public Event ReportEnd(ByVal sender As Object, _
      ByVal e As shippingDocRegPickEventArgs)

#End Region

#Region " Report Properties and Settings "

    Private mPageNumber As Integer
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

    Public Sub New()

        mFont = New Font("Arial", 10)
        mBrush = Brushes.Black
        FooterLines = 2
        MyBase.DefaultPageSettings.Margins.Top = 50
        MyBase.DefaultPageSettings.Landscape = True

        m_partialPickExcludeState = CNTR_STATE_EXCLUDE

    End Sub

    Public Sub New(ByVal srcbu As String, ByVal orderNo As String, ByVal strDB As String)

        mFont = New Font("Arial", 10)
        mBrush = Brushes.Black
        mSrcBU = srcbu
        mOrderNo = orderNo
        mDB = strDB
        MyBase.DefaultPageSettings.Margins.Top = 50
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
        MyBase.DefaultPageSettings.Margins.Top = 50
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
    Dim strPriority As String

    Private Sub ReportDocument1_BeginPrint(ByVal sender As Object, _
        ByVal e As System.Drawing.Printing.PrintEventArgs) Handles MyBase.BeginPrint

        mPageNumber = 0
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

        ' create our shippingDocRegPickEventArgs object for this page
        Dim page As New shippingDocRegPickEventArgs(e, mPageNumber, mFont, mBrush, mFooterLines)

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

    Private Sub PrintPageBodyStart(ByVal e As shippingDocRegPickEventArgs)

        Dim strSQLstring As String
        Dim I As Integer
        Dim Z As Integer
        Dim arrNotes As ArrayList = New ArrayList
        Dim arrDescr As ArrayList = New ArrayList
        Dim strFoot1 As String
        Dim strFoot2 As String
        Dim strFoot3 As String
        Dim strShipToDescr As String
        Dim strShipToAddress1 As String
        Dim strShipToAddress2 As String
        Dim strShipToAddress3 As String
        Dim strShipToAddress4 As String
        Dim strShipToCityState As String
        Dim strpreviousShipContainer As String
        Dim strcurrentShipContainer As String
        Dim intSavemY As Integer
        Dim bolCityState As Boolean


        Dim strNotes As String = ""

        ' since we added this (ISA_CUST_NOTES) as part of the main SELECT
        '   grab this from ds (instead of ds1) - Erwin 2015.09.14
        'If ds1.Tables(0).Rows.Count > 0 Then
        '    strNotes = ds1.Tables(0).Rows(0).Item("ISA_CUST_NOTES")
        'End If
        If (ds.Tables(0).Rows.Count > 0) Then
            Dim sep1 As String = ";"
            Dim sNote As String = ""
            For Each row As DataRow In ds.Tables(0).Rows
                sNote = ""
                Try
                    sNote = CStr(row("ISA_CUST_NOTES")).Trim.ToUpper
                Catch ex As Exception
                End Try
                If (sNote.Length > 0) Then
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

        ' generate the header
        PrintHeader(e)

        With e

            If ds.Tables(0).Rows.Count = 0 Then
                .CurrentF = arial10B
                .WriteLine("No data found for this order number! - " & mOrderNo, shippingDocRegPickLineJustification.Centered)
                Exit Sub
            End If

            .CurrentF = arial10
            .CurrentH = arial10.Height
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

            Else
                If mSrcBU = "I0206" Then  'gez 10/10/2012
                    strShipToDescr = (ds.Tables(0).Rows(mRow).Item("ISA_EMPLOYEE_ID") & " - " & _
                                   ds.Tables(0).Rows(mRow).Item("ISA_EMPLOYEE_NAME"))
                    'gez 10/10/2012
                    strShipToAddress1 = "" 'gez 10/10/2012
                    strShipToAddress2 = "" 'gez 10/10/2012
                    strShipToAddress3 = "" 'gez 10/10/2012
                    strShipToAddress4 = "" 'gez 10/10/2012r 
                    strShipToCityState = "" 'gez 10/10/2012
                    'strShipToDescr = CStr(ds2.Tables(0).Rows(0).Item("DESCR"))
                    'strShipToAddress1 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS1"))
                    'strShipToAddress2 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS2"))
                    'strShipToAddress3 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS3"))
                    'strShipToAddress4 = CStr(ds2.Tables(0).Rows(0).Item("ADDRESS4"))
                    'strShipToCityState = CStr(ds2.Tables(0).Rows(0).Item("CITY")) & ", " & _
                    '                     CStr(ds2.Tables(0).Rows(0).Item("STATE")) & " " & _
                    '                     CStr(ds2.Tables(0).Rows(0).Item("POSTAL"))

                End If


                'strShipToDescr = ds2.Tables(0).Rows(0).Item("DESCR")
                'strShipToAddress1 = ds2.Tables(0).Rows(0).Item("ADDRESS1")
                'strShipToAddress2 = ds2.Tables(0).Rows(0).Item("ADDRESS2")
                'strShipToAddress3 = ds2.Tables(0).Rows(0).Item("ADDRESS3")
                'strShipToAddress4 = ds2.Tables(0).Rows(0).Item("ADDRESS4")
                'strShipToCityState = ds2.Tables(0).Rows(0).Item("CITY") & ", " & _
                '    ds2.Tables(0).Rows(0).Item("STATE") & " " & _
                'ds2.Tables(0).Rows(0).Item("POSTAL")

            End If

            If e.PageNumber = 1 Then

                .Write("Order Date:", shippingDocRegPickLineJustification.col1)
                .Write(ds.Tables(0).Rows(mRow).Item("ORDER_DATE"), shippingDocRegPickLineJustification.col2)
                .Write("Deliver To...:", shippingDocRegPickLineJustification.col3)
                If mSrcBU = "I0206" Then 'gez 10/10/2012
                    .WriteLine(strShipToDescr, shippingDocRegPickLineJustification.col4)
                Else
                    .WriteLine(strShipToAddress1, shippingDocRegPickLineJustification.col4)
                End If
                '.Write("Deliver To:", shippingDocRegPickLineJustification.col3)
                '.WriteLine(strShipToDescr, shippingDocRegPickLineJustification.col4)
                .Write("Order Number:", shippingDocRegPickLineJustification.col1)
                .Write(ds.Tables(0).Rows(mRow).Item("ORDER_NO"), shippingDocRegPickLineJustification.col2)
                .WriteLine(strShipToAddress1, shippingDocRegPickLineJustification.col4)
                .Write("Work Order:", shippingDocRegPickLineJustification.col1)
                .Write(ds.Tables(0).Rows(mRow).Item("ISA_WORK_ORDER_NO"), shippingDocRegPickLineJustification.col2)
                If Not Trim(strShipToAddress2) = "" Then
                    .WriteLine(strShipToAddress2, shippingDocRegPickLineJustification.col4)
                ElseIf Not Trim(strShipToAddress3) = "" Then
                    .WriteLine(strShipToAddress3, shippingDocRegPickLineJustification.col4)
                ElseIf Not Trim(strShipToAddress4) = "" Then
                    .WriteLine(strShipToAddress4, shippingDocRegPickLineJustification.col4)
                Else
                    .WriteLine(strShipToCityState, shippingDocRegPickLineJustification.col4)
                    bolCityState = True
                End If
                '.Write("Shipping Container:", shippingDocRegPickLineJustification.col1)
                '.Write(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID"), shippingDocRegPickLineJustification.col2)
                .Write("Empl. ID:", shippingDocRegPickLineJustification.col1)
                .Write(ds.Tables(0).Rows(mRow).Item("ISA_EMPLOYEE_ID") & " - " & _
                    ds.Tables(0).Rows(mRow).Item("ISA_EMPLOYEE_NAME"), shippingDocRegPickLineJustification.col2)


                If bolCityState = False Then
                    If Not Trim(strShipToAddress3) = "" Then
                        .WriteLine(strShipToAddress3, shippingDocRegPickLineJustification.col4)
                    ElseIf Not Trim(strShipToAddress4) = "" Then
                        .WriteLine(strShipToAddress4, shippingDocRegPickLineJustification.col4)
                    Else
                        .WriteLine(strShipToCityState, shippingDocRegPickLineJustification.col4)
                        bolCityState = True
                    End If
                Else
                    .WriteLine()
                End If
                If bolCityState = False Then
                    If Not Trim(strShipToAddress4) = "" Then
                        .WriteLine(strShipToAddress4, shippingDocRegPickLineJustification.col4)
                    Else
                        .WriteLine(strShipToCityState, shippingDocRegPickLineJustification.col4)
                        bolCityState = True
                    End If
                End If
                If bolCityState = False Then
                    .WriteLine(strShipToCityState, shippingDocRegPickLineJustification.col4)
                    bolCityState = True
                End If

                ' don't use ds1 for the notes - Erwin 2015.09.14
                'If Not IsDBNull(ds1.Tables(0).Rows(0).Item("ISA_CUST_NOTES")) Then
                '    arrNotes = FormatWrap(e, strNotes.ToUpper, arial8, 900)
                'End If
                If (strNotes.Length > 0) Then
                    arrNotes = FormatWrap(e, strNotes.ToUpper, arial8, 900)
                End If

                .CurrentF = arial10B
                .CurrentH = arial10B.Height
                .WriteLine("Delivery Notes: ", shippingDocRegPickLineJustification.Left)
                .CurrentF = arial8
                .CurrentH = arial8.Height
                For I = 0 To arrNotes.Count - 1
                    .WriteLine(arrNotes(I))
                Next
                .HorizontalRule(1)
            End If
            .CurrentF = arial10B
            .CurrentH = arial10B.Height
            .Write("Ship", shippingDocRegPickLineJustification.colShipCntrID)
            .Write("Line", shippingDocRegPickLineJustification.colLineSched)
            .Write("Dem", shippingDocRegPickLineJustification.colDemSrc)
            .Write("Ship Date", shippingDocRegPickLineJustification.colShipDate)
            .Write("Item ID", shippingDocRegPickLineJustification.colItemID)
            .Write("Descr", shippingDocRegPickLineJustification.colDescr)
            .Write("Qty", shippingDocRegPickLineJustification.colQtyOrdered)
            .Write("Qty", shippingDocRegPickLineJustification.colQtyShipped)
            .WriteLine("Ord", shippingDocRegPickLineJustification.colOrdUOM)

            .Write("Cntr.", shippingDocRegPickLineJustification.colShipCntrID)
            .Write("Schd", shippingDocRegPickLineJustification.colLineSched)
            .Write("Src", shippingDocRegPickLineJustification.colDemSrc)
            .Write("Ordered", shippingDocRegPickLineJustification.colQtyOrdered)
            .Write("Shipped", shippingDocRegPickLineJustification.colQtyShipped)
            .WriteLine("UOM", shippingDocRegPickLineJustification.colOrdUOM)
            .HorizontalRule(1)

            .CurrentF = arial10
            .CurrentH = arial10.Height

            Dim arrItemIDA As ArrayList
            Dim arrItemIDB As ArrayList
            Dim intIndex As Integer

            arrItemIDA = New ArrayList
            arrItemIDB = New ArrayList

            If ds.Tables(0).Rows.Count > 0 Then
                For I = 0 To ds.Tables(0).Rows.Count - 1
                    intIndex = arrItemIDA.IndexOf(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                    If intIndex = -1 Then
                        arrItemIDA.Add(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                    Else
                        intIndex = arrItemIDB.IndexOf(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                        If intIndex = -1 Then
                            arrItemIDB.Add(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                        End If
                    End If
                Next
                For I = mRow To ds.Tables(0).Rows.Count - 1
                    arrDescr = FormatWrap(e, Regex.Replace(ds.Tables(0).Rows(I).Item("DESCR254"), "\s+", " ").ToUpper, arial10, 250)
                    If .CurrentY + 16 + (16 * arrDescr.Count) > e.PageBottom Then
                        e.HasMorePages = True
                        Exit Sub
                    End If
                    If IsDBNull(ds.Tables(0).Rows(I).Item("SHIP_CNTR_ID")) Then
                        strcurrentShipContainer = " "
                    Else
                        strcurrentShipContainer = ds.Tables(0).Rows(I).Item("SHIP_CNTR_ID")
                    End If

                    Dim bIsCntrTypeWritten As Boolean = False
                    Dim bIsCntrIdWritten As Boolean = False
                    If Not strpreviousShipContainer = strcurrentShipContainer Then
                        .HorizontalRule(2)
                        .Write(Convert.ToString(ds.Tables(0).Rows(I).Item("SHIP_CNTR_ID")), shippingDocRegPickLineJustification.colShipCntrID)
                        bIsCntrIdWritten = True
                    End If
                    strpreviousShipContainer = strcurrentShipContainer

                    .Write((ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO") & " - " & ds.Tables(0).Rows(I).Item("SCHED_LINE_NO")), shippingDocRegPickLineJustification.colLineSched)
                    intIndex = arrItemIDB.IndexOf(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                    If intIndex > -1 Then
                        .Write("*")
                    End If
                    .Write(Convert.ToString(ds.Tables(0).Rows(I).Item("DEMAND_SOURCE")), shippingDocRegPickLineJustification.colDemSrc)
                    .Write(Convert.ToString(ds.Tables(0).Rows(I).Item("PICK_DATE")), shippingDocRegPickLineJustification.colShipDate)
                    .Write(ds.Tables(0).Rows(I).Item("INV_ITEM_ID"), shippingDocRegPickLineJustification.colItemID)
                    .Write(arrDescr(0), shippingDocRegPickLineJustification.colDescr)
                    intIndex = arrItemIDC.IndexOf(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                    If intIndex = -1 Then
                        .Write(Convert.ToString(ds.Tables(0).Rows(I).Item("QTY_ORDERED")), shippingDocRegPickLineJustification.colQtyOrdered)
                        .Write(Convert.ToString(ds.Tables(0).Rows(I).Item("QTY_PICKED")), shippingDocRegPickLineJustification.colQtyShipped)
                    Else
                        .Write("--", shippingDocRegPickLineJustification.colQtyOrdered)
                        .Write("--", shippingDocRegPickLineJustification.colQtyShipped)
                    End If

                    intIndex = arrItemIDB.IndexOf(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                    If intIndex > -1 Then
                        intIndex = arrItemIDC.IndexOf(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                        If intIndex = -1 Then
                            arrItemIDC.Add(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                        End If
                    End If

                    .Write(ds.Tables(0).Rows(I).Item("UNIT_OF_MEASURE"), shippingDocRegPickLineJustification.colOrdUOM)
                    .WriteLine()
                    If bIsCntrIdWritten Then
                        Dim containerType As String = ""
                        Try
                            If Not IsDBNull(ds.Tables(0).Rows(I).Item("CONTAINER_TYPE")) Then
                                containerType = GetContainerTypeName(CStr(ds.Tables(0).Rows(I).Item("CONTAINER_TYPE")))
                            End If
                        Catch ex As Exception
                        End Try
                        .Write(containerType, shippingDocRegPickLineJustification.colShipCntrID)
                        bIsCntrTypeWritten = True
                    End If
                    If Not Trim(ds.Tables(0).Rows(I).Item("ISA_USER_DEFINED_3")) = "" Then
                        .Write("Staged Loc - " & ds.Tables(0).Rows(I).Item("ISA_USER_DEFINED_3"), shippingDocRegPickLineJustification.colLineSched)
                    End If

                    If arrDescr.Count > 1 Then
                        .WriteLine(arrDescr(1), shippingDocRegPickLineJustification.colDescr)
                    End If

                    If bIsCntrTypeWritten Then
                        Dim s As String = ""
                        Try
                            s = CStr(ds.Tables(0).Rows(I).Item("ISA_CUST_NOTES")).Trim
                        Catch ex As Exception
                        End Try
                        If (s.Length > 0) Then
                            .Write(s, shippingDocRegPickLineJustification.colShipCntrID)
                        End If
                    End If

                    If arrDescr.Count > 2 Then
                        For Z = 2 To arrDescr.Count - 1
                            .WriteLine(arrDescr(Z), shippingDocRegPickLineJustification.colDescr)
                        Next
                    Else
                        If Not Trim(ds.Tables(0).Rows(I).Item("ISA_USER_DEFINED_3")) = "" Then
                            .WriteLine()
                        End If
                    End If

                    If I = ds.Tables(0).Rows.Count - 1 Then
                        .HorizontalRule(2)
                    End If

                    mRow += 1

                Next

            End If
            'If Not IsDBNull(ds.Tables(0).Rows(mRow - 1).Item("SHIP_CNTR_ID")) Then
            '    PrintDocumentOnPrintPageFromDLL(e, ds.Tables(0).Rows(mRow - 1).Item("ORDER_NO"))
            'End If

        End With
    End Sub

    Private Sub DrawRectangleFloat(ByVal e As Printing.PrintPageEventArgs, ByVal column As Integer, ByVal line As Integer, ByVal width As Integer, ByVal Height As Integer)
        ' Create pen.
        Dim blackPen As New Pen(Color.Black, 3)
        e.Graphics.DrawRectangle(blackPen, column, line, width, Height)
    End Sub

    Private Sub GeneratePage(ByVal e As shippingDocRegPickEventArgs)

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

    Private Function FormatWrap(ByVal e As shippingDocRegPickEventArgs, ByVal currentText As String, ByVal pFont As Font, ByVal fieldsize As Integer) As ArrayList

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
            decDiv = lengthOfText / fieldsize
            maxLengthOfALine = endingPosition / decDiv
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

        Dim rtn As String = "shippingDocRegPick.getShipto"

        Dim strSQLstring As String = "" & _
                    "SELECT A.DESCR, A.ADDRESS1, A.ADDRESS2," & vbCrLf & _
                    " A.ADDRESS3, A.ADDRESS4, A.CITY, A.STATE, A.POSTAL" & vbCrLf & _
                    " FROM PS_LOCATION_TBL A" & vbCrLf & _
                    " WHERE A.EFFDT =" & vbCrLf & _
                    " (SELECT MAX(A_ED.EFFDT) FROM PS_LOCATION_TBL A_ED" & vbCrLf & _
                    " WHERE A.SETID = A_ED.SETID" & vbCrLf & _
                    " AND A.LOCATION = A_ED.LOCATION" & vbCrLf & _
                    " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
                    " AND A.LOCATION = '" & strShipto & "'" & vbCrLf & _
                    " AND A.EFF_STATUS = 'A'" & vbCrLf & _
                    ""

        'Dim dbConn As String = _
        '"Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=" & mDB

        LogVerbose(msg:=rtn & "::CN string : " & Me.oraCNstring)
        LogVerbose(msg:=rtn & "::executing : " & strSQLstring)

        ''"data source=ineroth;initial catalog=pubs;integrated security=SSPI"
        'Dim da As New OleDb.OleDbDataAdapter(strSQLstring, dbConn)
        Dim da As New OleDb.OleDbDataAdapter(strSQLstring, Me.oraCNstring)
        Dim dsAddr As New DataSet
        da.Fill(dsAddr)
        Return dsAddr

    End Function

    Private Function getTextLength(ByVal e As shippingDocRegPickEventArgs, ByVal currentText As String, ByVal pFont As Font) As Integer

        getTextLength = CInt(e.Graphics.MeasureString(currentText, pFont).Width)

    End Function

    Private Sub PrintHeader(ByVal e As shippingDocRegPickEventArgs)

        Dim rtn As String = "shippingDoc.PrintHeader"

        Dim field As Integer
        Dim strSQLstring As String
        Dim intContainers As Integer
        If e.PageNumber = 1 Then
            '    Dim dbConn As String = _
            '"Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=" & mDB

            strSQLstring = "SELECT DISTINCT A.SHIP_CNTR_ID" & vbCrLf & _
                        " FROM PS_ISA_PICKING_CNT A, PS_ISA_PICKING_INT B" & vbCrLf & _
                        " WHERE A.SOURCE_BUS_UNIT = '" & mSrcBU & "'" & vbCrLf & _
                        " AND A.ORDER_NO = '" & mOrderNo & "'" & vbCrLf & _
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
                        " AND A.RECV_LN_NBR = B.RECV_LN_NBR" & vbCrLf & _
                        " AND B.ENDDTTM IS NOT NULL" & vbCrLf & _
                        " AND B.QTY_PICKED > 0" & vbCrLf
            If (Me.PartialPickExcludeState.Trim.Length > 0) Then
                strSQLstring = strSQLstring & " AND B.ISA_USER_DEFINED_1 NOT IN (" & Me.PartialPickExcludeState & ") " & vbCrLf
            End If
            If Not mOverRide = "Y" Then
                strSQLstring = strSQLstring & " AND B.SHIP_DTTM IS NULL" & vbCrLf
            End If

            LogVerbose(msg:=rtn & "::CN string : " & Me.oraCNstring)
            LogVerbose(msg:=rtn & "::executing : " & strSQLstring)

            'Dim da3 As New OleDb.OleDbDataAdapter(strSQLstring, dbConn)
            Dim da3 As New OleDb.OleDbDataAdapter(strSQLstring, Me.oraCNstring)

            da3.Fill(ds3)
            intContainers = ds3.Tables(0).Rows.Count
            strSQLstring = "SELECT (B.ISA_APPROVAL_CD || '-' || B.ISA_DESC) as PDESC" & vbCrLf & _
                        " FROM PS_ISA_PICKING_INT A, PS_ISA_ORD_INTFC_M B" & vbCrLf & _
                        " WHERE A.SOURCE_BUS_UNIT = '" & mSrcBU & "'" & vbCrLf & _
                        " AND A.ORDER_NO = '" & mOrderNo & "'" & vbCrLf & _
                        " AND A.REFERENCE_ID = B.ORDER_NO" & vbCrLf & _
                        " AND A.ORDER_INT_LINE_NO = B.LINE_NBR" & vbCrLf & _
                        " AND ROWNUM < 2"

            'Dim dbConn2 As New System.Data.OleDb.OleDbConnection(dbConn)
            Dim dbConn2 As New System.Data.OleDb.OleDbConnection(Me.oraCNstring)
            dbConn2.Open()

            Dim dr2 As New OleDb.OleDbCommand(strSQLstring, dbConn2)
            strPriority = dr2.ExecuteScalar
        End If

        With e

            .CurrentF = arial12B
            .CurrentH = arial12B.Height
            .Write(("Priority: " & strPriority), shippingDocRegPickLineJustification.Right)
            .CurrentF = arial20B
            .CurrentH = arial20B.Height
            .WriteLine("SDI, Inc. ", shippingDocRegPickLineJustification.Centered)
            If e.PageNumber = 1 Then
                .Write("SHIPPING DOCUMENT", shippingDocRegPickLineJustification.Centered)
                .CurrentF = arial12
                .CurrentH = arial12.Height
                .Write("Total Containers = " & intContainers, shippingDocRegPickLineJustification.Right)
                .CurrentF = arial20B
                .CurrentH = arial20B.Height
                .WriteLine()
            Else
                .WriteLine("SHIPPING DOCUMENT", shippingDocRegPickLineJustification.Centered)
            End If


            ' display a horizontal line to separate the header from
            ' the body
            .HorizontalRule(1)
        End With

    End Sub

    Private Sub PrintFooter(ByVal e As shippingDocRegPickEventArgs)

        With e

            If mRow >= ds.Tables(0).Rows.Count Then
                'If .CurrentY + 70 > e.PageBottom Then
                If .CurrentY + 60 > e.PageBottom Then
                    e.HasMorePages = True
                Else
                    ' container count
                    Dim arrContainerTypeCount As New Hashtable
                    Dim containerType As String = ""
                    Dim arrCntrId As New ArrayList
                    Dim cntrId As String = ""
                    For Each row As DataRow In ds.Tables(0).Rows
                        cntrId = ""
                        Try
                            If Not IsDBNull(row("SHIP_CNTR_ID")) Then
                                cntrId = CStr(row("SHIP_CNTR_ID")).Trim
                            End If
                        Catch ex As Exception
                        End Try
                        containerType = ""
                        Try
                            If Not IsDBNull(row("CONTAINER_TYPE")) Then
                                containerType = CStr(row("CONTAINER_TYPE")).Trim
                            End If
                        Catch ex As Exception
                        End Try
                        If containerType.Trim.Length = 0 Then
                            containerType = "MISC"
                        End If
                        Try
                            Dim arr As ArrayList = Nothing
                            If arrContainerTypeCount.ContainsKey(key:=containerType) Then
                                arr = CType(arrContainerTypeCount(key:=containerType), ArrayList)
                                Dim bExists As Boolean = False
                                For Each s As String In arr
                                    If s = cntrId Then
                                        bExists = True
                                        Exit For
                                    End If
                                Next
                                If Not bExists Then
                                    arr.Add(cntrId)
                                End If
                            Else
                                arr = New ArrayList
                                arr.Add(cntrId)
                                arrContainerTypeCount.Add(key:=containerType, value:=arr)
                            End If
                        Catch ex As Exception
                        End Try
                    Next
                    .CurrentF = arial9
                    .CurrentH = arial9.Height
                    .CurrentY = (e.PageBottom - 80)
                    Dim strSummary As String = ""
                    Dim s2 As String = ""
                    Dim arr2 As New ArrayList
                    For Each s1 As String In arrContainerTypeCount.Keys
                        s2 = Me.GetContainerTypeName(s1)
                        'strSummary &= s2 & " - " & CType(arrContainerTypeCount(s1), ArrayList).Count.ToString & "; "
                        arr2.Add(s2 & " - " & CType(arrContainerTypeCount(s1), ArrayList).Count.ToString)
                    Next
                    arr2.Sort()
                    For Each s As String In arr2
                        strSummary &= s & "; "
                    Next
                    '.WriteLine(strSummary, shippingDocRegPickLineJustification.colShipCntrID)
                    .WriteLine("SUMMARY:  " & strSummary, 105)

                    .CurrentF = arial12
                    .CurrentH = arial12.Height
                    .CurrentY = (e.PageBottom - 43)
                    .WriteLine("Sign here: ______________________________________________", 105)

                    DrawRectangleFloat(e, .MarginBounds.Left, (.PageBottom - 70), 650, 65)
                    'DrawRectangleFloat(e, 25, 50, 650, 15)
                End If
            End If
            ' set our vertical position to the top of the footer region
            .CurrentY = e.PageBottom

            .CurrentF = arial8
            .CurrentH = arial8.Height
            .HorizontalRule(1)

            ' write the left-side footer text
            If Len(mFooterLeft) > 0 Then
                .Write(mFooterLeft)
            Else
                ' we default to displaying the current date
                '.Write(Format(Now, "Short date"))
                .Write(Now().ToString)
                .Write(mOrderNo)
                .Write("* indicates item is in multiple packages.", shippingDocRegPickLineJustification.Centered)
            End If

            ' write the right-side footer text
            If Len(mFooterRight) > 0 Then
                .WriteLine(mFooterRight)

            Else
                ' we default to displaying the current page number
                .WriteLine("Page " & e.PageNumber, shippingDocRegPickLineJustification.Right)
            End If
            If e.PageNumber = 1 Then
                PrintDocumentOnPrintPageFromDLL(e, mOrderNo)
            End If


        End With

    End Sub

    Private Sub buildDatasets()

        Dim rtn As String = "shippingDocRegPick.buildDatasets"

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
                        ", VW.ISA_USER_DEFINED_3  " & vbCrLf & _
                        ", SUM (VW.QTY_PICKED) AS QTY_PICKED " & vbCrLf & _
                        ", VW.DEMAND_SOURCE  " & vbCrLf & _
                        ", VW.UNIT_OF_MEASURE  " & vbCrLf & _
                        ", VW.QTY_ORDERED  " & vbCrLf & _
                        ", VW.ORDER_DATE  " & vbCrLf & _
                        ", VW.ISA_EMPLOYEE_ID  " & vbCrLf & _
                        ", VW.ISA_EMPLOYEE_NAME  " & vbCrLf & _
                        ", VW.CONTAINER_TYPE " & vbCrLf & _
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
                        "     , A.ISA_USER_DEFINED_3 " & vbCrLf & _
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
                        "     , B.CONTAINER_TYPE " & vbCrLf & _
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
                        "    , A.ISA_USER_DEFINED_3  " & vbCrLf & _
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
                        "    , B.CONTAINER_TYPE " & vbCrLf & _
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
                        ", VW.ISA_USER_DEFINED_3  " & vbCrLf & _
                        ", VW.DEMAND_SOURCE  " & vbCrLf & _
                        ", VW.UNIT_OF_MEASURE  " & vbCrLf & _
                        ", VW.QTY_ORDERED  " & vbCrLf & _
                        ", VW.ORDER_DATE  " & vbCrLf & _
                        ", VW.ISA_EMPLOYEE_ID  " & vbCrLf & _
                        ", VW.ISA_EMPLOYEE_NAME  " & vbCrLf & _
                        ", VW.CONTAINER_TYPE  " & vbCrLf & _
                        ", VW.ISA_CUST_NOTES " & vbCrLf & _
                        "ORDER BY  " & vbCrLf & _
                        "  VW.SHIP_CNTR_ID " & vbCrLf & _
                        ", VW.ORDER_INT_LINE_NO " & vbCrLf & _
                        ", VW.SCHED_LINE_NO " & vbCrLf & _
                       ""

        LogVerbose(msg:=rtn & "::CN string : " & Me.oraCNstring)
        LogVerbose(msg:=rtn & "::executing : " & strSQLstring)

        Dim da As New OleDb.OleDbDataAdapter(strSQLstring, Me.oraCNstring)
        da.Fill(ds)

        ' fill the "printed picked order lines" and raise the event
        FillPrintedPickedOrderLinesFromDS(ds)

        ' NOTE:
        '   (1) get the first available "shipto_id" of the first available STK item
        '       if NOT exist, get the first available "shipto_id" of the first available NSTK item
        '       to handle non-stock ONLY orders
        '   - erwin 2009.07.24
        strSQLstring = "" & _
             "SELECT B.ISA_CUST_NOTES, B.SHIPTO_ID " & vbCrLf & _
             "FROM PS_ISA_ORD_INTFC_H A, PS_ISA_ORD_INTFC_L B " & vbCrLf & _
             "WHERE A.BUSINESS_UNIT_OM = '" & mSrcBU & "'" & vbCrLf & _
             "  AND A.ORDER_NO = '" & mOrderNo & "'" & vbCrLf & _
             "  AND A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
             "  AND B.INV_ITEM_ID <> ' '" & vbCrLf & _
             "  AND ROWNUM < 2" & vbCrLf & _
             ""

        LogVerbose(msg:=rtn & "::CN string : " & Me.oraCNstring)
        LogVerbose(msg:=rtn & "::executing : " & strSQLstring)

        ''Dim da1 As New OleDb.OleDbDataAdapter(strSQLstring, dbConn)
        'Dim da1 As New OleDb.OleDbDataAdapter(strSQLstring, Me.oraCNstring)
        Dim da1 As OleDb.OleDbDataAdapter = Nothing

        da1 = New OleDb.OleDbDataAdapter(strSQLstring, Me.oraCNstring)

        da1.Fill(ds1)
        If ds1.Tables(0).Rows.Count > 0 Then
            ds2 = getShipto(ds1.Tables(0).Rows(0).Item("SHIPTO_ID"))
        Else
            'ds2 = getShipto(" ")
            strSQLstring = "" & _
                 "SELECT B.ISA_CUST_NOTES, B.SHIPTO_ID " & vbCrLf & _
                 "FROM PS_ISA_ORD_INTFC_H A, PS_ISA_ORD_INTFC_L B " & vbCrLf & _
                 "WHERE A.BUSINESS_UNIT_OM = '" & mSrcBU & "'" & vbCrLf & _
                 "  AND A.ORDER_NO = '" & mOrderNo & "'" & vbCrLf & _
                 "  AND A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
                 "  AND B.INV_ITEM_ID = ' '" & vbCrLf & _
                 "  AND ROWNUM < 2" & vbCrLf & _
                 ""
            LogVerbose(msg:=rtn & "::CN string : " & Me.oraCNstring)
            LogVerbose(msg:=rtn & "::executing : " & strSQLstring)
            da1 = New OleDb.OleDbDataAdapter(strSQLstring, Me.oraCNstring)
            da1.Fill(ds1)
            If ds1.Tables(0).Rows.Count > 0 Then
                ds2 = getShipto(ds1.Tables(0).Rows(0).Item("SHIPTO_ID"))
            Else
                ds2 = getShipto(" ")
            End If
        End If

    End Sub

    Private Sub PrintDocumentOnPrintPageFromDLL(ByVal e As PrintPageEventArgs, ByVal strOrderNO As String)
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

        NewBarcode.DataToEncode = strOrderNO
        NewBarcode.SymbologyID = NewBarcode.Symbologies.Code128

        myImage = NewBarcode.Picture

        grfx.ResetTransform()
        grfx.DrawImage(myImage, 25, 5)
    End Sub

#End Region

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

End Class
