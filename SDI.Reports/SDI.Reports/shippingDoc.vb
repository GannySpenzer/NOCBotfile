Imports System.Drawing
Imports System.Drawing.Printing
Imports System.Text.RegularExpressions
Imports System.Xml


Public Class shippingDoc

    Inherits PrintDocument

    Private Const oraCN_default_provider As String = "Provider=MSDAORA.1;"
    Private Const oraCN_default_creden As String = "User ID=einternet;Password=einternet;"
    Private Const oraCN_default_DB As String = "Data Source=prod"

    ''This line was added to allow printing as a DLL
    'Protected WithEvents LinearBarcode1 As IDAutomation.LinearServerControl.LinearBarcode
    Private WithEvents linearBarcoder As IDAutomation.LinearServerControl.LinearBarcode = Nothing

    Private mPageNumber As Integer = 0
    Private mTitle As String = ""
    Private mSubTitleLeft As String = ""
    Private mSubTitleLeft1 As String = ""
    Private mSubTitleRight As String = ""
    Private mFooterLeft As String = ""
    Private mFooterRight As String = ""
    Private mFont As Font = Nothing
    Private mBrush As Brush = Nothing
    Private mFooterLines As Integer = 0
    Private mSrcBU As String = ""
    'Private mOverRide As String = ""
    Private mOrderNo As String = ""
    'Private mDB As String = ""
    Private arial30B As New Font("Arial", 30, FontStyle.Bold)
    Private arial20B As New Font("Arial", 20, FontStyle.Bold)
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
    Private arrItemIDC As ArrayList = Nothing
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


#Region " Event declarations "

    ''' <summary>
    ''' Raised once immediately before anything is printed to the report. The cursor is on the first line of the first page.
    ''' </summary>
    Public Event ReportBegin(ByVal sender As Object, _
                             ByVal e As shippingDocPageEventArgs)
    ''' <summary>
    ''' Raised for each page immediately before anything is printed to that page. The cursor is on the first line of the page.
    ''' </summary>
    Public Event PrintPageBegin(ByVal sender As Object, _
                                ByVal e As shippingDocPageEventArgs)
    ''' <summary>
    ''' Raised for each page immediately after the header for the page has been printed. The cursor is on the first line of the report body.
    ''' </summary>
    Public Event PrintPageBodyEnd(ByVal sender As Object, _
                                  ByVal e As shippingDocPageEventArgs)
    ''' <summary>
    ''' Raised for each page after the footer has been printed. The cursor is past the end of the footer, typically into the bottom margin of the page.
    ''' </summary>
    Public Event PrintPageEnd(ByVal sender As Object, _
                              ByVal e As shippingDocPageEventArgs)
    ''' <summary>
    ''' Raised once at the very end of the report after all other printing is complete. The cursor is past the end of the footer on the last page, typically into the bottom margin of the page.
    ''' </summary>
    Public Event ReportEnd(ByVal sender As Object, _
                           ByVal e As shippingDocPageEventArgs)

#End Region

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

        MyBase.DefaultPageSettings.Margins.Top = 50
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
        components = New System.ComponentModel.Container
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

#Region " Do printing "

    Dim mRow As Integer
    Dim strPriority As String

    Private Sub ReportDocument1_BeginPrint(ByVal sender As Object, _
                                           ByVal e As System.Drawing.Printing.PrintEventArgs) Handles MyBase.BeginPrint

        mPageNumber = 0
        mRow = 0
        buildDatasets()

    End Sub

    Private Sub ReportDocument1_PrintPage(ByVal sender As Object, _
                                          ByVal e As System.Drawing.Printing.PrintPageEventArgs) Handles MyBase.PrintPage

        mPageNumber += 1

        ' create our ReportPageEventArgs1 object for this page
        Dim page As New shippingDocPageEventArgs(e, mPageNumber, mFont, mBrush, mFooterLines)

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

    Private Sub PrintPageBodyStart(ByVal e As shippingDocPageEventArgs)

        Dim strSQLstring As String
        Dim I As Integer
        Dim Z As Integer
        Dim arrNotes As ArrayList
        Dim arrDescr As ArrayList
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

        Dim strNotes As String

        If ds1.Tables(0).Rows.Count > 0 Then
            strNotes = ds1.Tables(0).Rows(0).Item("ISA_CUST_NOTES")
        End If

        ' generate the header
        PrintHeader(e)

        With e

            If ds.Tables(0).Rows.Count = 0 Then
                .CurrentF = arial10B
                .WriteLine("No data found for this order number! - " & mOrderNo, shippingDocRptLineJustification.Centered)
                Exit Sub
            End If

            .CurrentF = arial10
            .CurrentH = arial10.Height
            If ds2.Tables(0).Rows.Count > 0 Then
                strShipToDescr = ds2.Tables(0).Rows(0).Item("DESCR")
                strShipToAddress1 = ds2.Tables(0).Rows(0).Item("ADDRESS1")
                strShipToAddress2 = ds2.Tables(0).Rows(0).Item("ADDRESS2")
                strShipToAddress3 = ds2.Tables(0).Rows(0).Item("ADDRESS3")
                strShipToAddress4 = ds2.Tables(0).Rows(0).Item("ADDRESS4")
                strShipToCityState = ds2.Tables(0).Rows(0).Item("CITY") & ", " & _
                    ds2.Tables(0).Rows(0).Item("STATE") & " " & _
                    ds2.Tables(0).Rows(0).Item("POSTAL")

            End If

            If e.PageNumber = 1 Then

                .Write("Order Date:", shippingDocRptLineJustification.col1)
                .Write(ds.Tables(0).Rows(mRow).Item("ORDER_DATE"), shippingDocRptLineJustification.col2)
                .Write("Deliver To...:9", shippingDocRptLineJustification.col3)
                .WriteLine(strShipToDescr, shippingDocRptLineJustification.col4)
                .Write("Order Number:", shippingDocRptLineJustification.col1)
                .Write(ds.Tables(0).Rows(mRow).Item("ORDER_NO"), shippingDocRptLineJustification.col2)
                .WriteLine(strShipToAddress1, shippingDocRptLineJustification.col4)
                .Write("Work Order:", shippingDocRptLineJustification.col1)
                .Write(ds.Tables(0).Rows(mRow).Item("ISA_WORK_ORDER_NO"), shippingDocRptLineJustification.col2)
                If Not Trim(strShipToAddress2) = "" Then
                    .WriteLine(strShipToAddress2, shippingDocRptLineJustification.col4)
                ElseIf Not Trim(strShipToAddress3) = "" Then
                    .WriteLine(strShipToAddress3, shippingDocRptLineJustification.col4)
                ElseIf Not Trim(strShipToAddress4) = "" Then
                    .WriteLine(strShipToAddress4, shippingDocRptLineJustification.col4)
                Else
                    .WriteLine(strShipToCityState, shippingDocRptLineJustification.col4)
                    bolCityState = True
                End If
                '.Write("Shipping Container:", shippingDocRptLineJustification.col1)
                '.Write(ds.Tables(0).Rows(mRow).Item("SHIP_CNTR_ID"), shippingDocRptLineJustification.col2)
                .Write("Empl. ID:", shippingDocRptLineJustification.col1)
                .Write(ds.Tables(0).Rows(mRow).Item("ISA_EMPLOYEE_ID") & " - " & _
                    ds.Tables(0).Rows(mRow).Item("ISA_EMPLOYEE_NAME"), shippingDocRptLineJustification.col2)


                If bolCityState = False Then
                    If Not Trim(strShipToAddress3) = "" Then
                        .WriteLine(strShipToAddress3, shippingDocRptLineJustification.col4)
                    ElseIf Not Trim(strShipToAddress4) = "" Then
                        .WriteLine(strShipToAddress4, shippingDocRptLineJustification.col4)
                    Else
                        .WriteLine(strShipToCityState, shippingDocRptLineJustification.col4)
                        bolCityState = True
                    End If
                Else
                    .WriteLine()
                End If
                If bolCityState = False Then
                    If Not Trim(strShipToAddress4) = "" Then
                        .WriteLine(strShipToAddress4, shippingDocRptLineJustification.col4)
                    Else
                        .WriteLine(strShipToCityState, shippingDocRptLineJustification.col4)
                        bolCityState = True
                    End If
                End If
                If bolCityState = False Then
                    .WriteLine(strShipToCityState, shippingDocRptLineJustification.col4)
                    bolCityState = True
                End If
                If Not IsDBNull(ds1.Tables(0).Rows(0).Item("ISA_CUST_NOTES")) Then
                    arrNotes = FormatWrap(e, strNotes.ToUpper, arial8, 900)
                End If

                .CurrentF = arial10B
                .CurrentH = arial10B.Height
                .WriteLine("Delivery Notes: ", shippingDocRptLineJustification.Left)
                .CurrentF = arial8
                .CurrentH = arial8.Height
                For I = 0 To arrNotes.Count - 1
                    .WriteLine(arrNotes(I))
                Next
                .HorizontalRule(1)
            End If
            .CurrentF = arial10B
            .CurrentH = arial10B.Height
            .Write("Ship", shippingDocRptLineJustification.colShipCntrID)
            .Write("Line", shippingDocRptLineJustification.colLineSched)
            .Write("Dem", shippingDocRptLineJustification.colDemSrc)
            .Write("Ship Date", shippingDocRptLineJustification.colShipDate)
            .Write("Item ID", shippingDocRptLineJustification.colItemID)
            .Write("Descr", shippingDocRptLineJustification.colDescr)
            .Write("Qty", shippingDocRptLineJustification.colQtyOrdered)
            .Write("Qty", shippingDocRptLineJustification.colQtyShipped)
            .WriteLine("Ord", shippingDocRptLineJustification.colOrdUOM)

            .Write("Cntr.", shippingDocRptLineJustification.colShipCntrID)
            .Write("Schd", shippingDocRptLineJustification.colLineSched)
            .Write("Src", shippingDocRptLineJustification.colDemSrc)
            .Write("Ordered", shippingDocRptLineJustification.colQtyOrdered)
            .Write("Shipped", shippingDocRptLineJustification.colQtyShipped)
            .WriteLine("UOM", shippingDocRptLineJustification.colOrdUOM)
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

                    If Not strpreviousShipContainer = strcurrentShipContainer Then
                        .HorizontalRule(2)
                        .Write(Convert.ToString(ds.Tables(0).Rows(I).Item("SHIP_CNTR_ID")), shippingDocRptLineJustification.colShipCntrID)
                    End If
                    strpreviousShipContainer = strcurrentShipContainer

                    .Write((ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO") & " - " & ds.Tables(0).Rows(I).Item("SCHED_LINE_NO")), shippingDocRptLineJustification.colLineSched)
                    intIndex = arrItemIDB.IndexOf(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                    If intIndex > -1 Then
                        .Write("*")
                    End If
                    .Write(Convert.ToString(ds.Tables(0).Rows(I).Item("DEMAND_SOURCE")), shippingDocRptLineJustification.colDemSrc)
                    .Write(Convert.ToString(ds.Tables(0).Rows(I).Item("PICK_DATE")), shippingDocRptLineJustification.colShipDate)
                    .Write(ds.Tables(0).Rows(I).Item("INV_ITEM_ID"), shippingDocRptLineJustification.colItemID)
                    .Write(arrDescr(0), shippingDocRptLineJustification.colDescr)

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

                    intIndex = arrItemIDC.IndexOf(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                    If intIndex = -1 Then
                        Try
                            'nOrderedAndPickedDiff = (CInt(ds.Tables(0).Rows(I).Item("QTY_ORDERED")) - ds.Tables(0).Rows(I).Item("QTY_PICKED"))
                            nOrderedAndPickedDiff = (myQty.QtyRequested - ds.Tables(0).Rows(I).Item("QTY_PICKED"))
                        Catch ex As Exception
                        End Try
                        '.Write(Convert.ToString(ds.Tables(0).Rows(I).Item("QTY_ORDERED")), shippingDocRptLineJustification.colQtyOrdered)
                        .Write(Convert.ToString(myQty.QtyRequested), shippingDocRptLineJustification.colQtyOrdered)
                        .Write(Convert.ToString(ds.Tables(0).Rows(I).Item("QTY_PICKED")), shippingDocRptLineJustification.colQtyShipped)
                    Else
                        nOrderedAndPickedDiff = 0
                        .Write("--", shippingDocRptLineJustification.colQtyOrdered)
                        .Write("--", shippingDocRptLineJustification.colQtyShipped)
                    End If

                    ' put-away location is any
                    Dim sPutawayLoc As String = ""
                    Dim putawayLoc As String() = New String() {"", "", "", "", ""}
                    Try
                        putawayLoc(0) = CStr(ds.Tables(0).Rows(I).Item("STORAGE_AREA"))
                    Catch ex As Exception
                    End Try
                    Try
                        putawayLoc(1) = CStr(ds.Tables(0).Rows(I).Item("STOR_LEVEL_1"))
                    Catch ex As Exception
                    End Try
                    Try
                        putawayLoc(2) = CStr(ds.Tables(0).Rows(I).Item("STOR_LEVEL_2"))
                    Catch ex As Exception
                    End Try
                    Try
                        putawayLoc(3) = CStr(ds.Tables(0).Rows(I).Item("STOR_LEVEL_3"))
                    Catch ex As Exception
                    End Try
                    Try
                        putawayLoc(4) = CStr(ds.Tables(0).Rows(I).Item("STOR_LEVEL_4")) & _
                                        " (" & CStr(ds.Tables(0).Rows(I).Item("DESTINATION_BU")) & ")"
                    Catch ex As Exception
                    End Try
                    If putawayLoc(0).Trim.Length = 0 And _
                       putawayLoc(1).Trim.Length = 0 And _
                       putawayLoc(2).Trim.Length = 0 And _
                       putawayLoc(3).Trim.Length = 0 And _
                       putawayLoc(4).Trim.Length = 0 Then
                        ' no location for this item
                        sPutawayLoc = "*** NOT DEFINED ***"
                    Else
                        ' with location so, format location
                        sPutawayLoc = "" & _
                                      putawayLoc(0).Trim & " " & _
                                      putawayLoc(1).Trim & " " & _
                                      putawayLoc(2).Trim & " " & _
                                      putawayLoc(3).Trim & " " & _
                                      putawayLoc(4).Trim & _
                                      ""
                    End If

                    ' notice
                    Dim sNotice As String = ""

                    If nOrderedAndPickedDiff < 0 Then
                        sNotice = "" & _
                                  "*** WARNING:  PLEASE NOTE PICKED QTY IS GREATER THAN REQUESTED ***" & _
                                  ""
                    End If

                    intIndex = arrItemIDB.IndexOf(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                    If intIndex > -1 Then
                        intIndex = arrItemIDC.IndexOf(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                        If intIndex = -1 Then
                            arrItemIDC.Add(ds.Tables(0).Rows(I).Item("INV_ITEM_ID") & ds.Tables(0).Rows(I).Item("ORDER_INT_LINE_NO"))
                        End If
                    End If

                    .Write(ds.Tables(0).Rows(I).Item("UNIT_OF_MEASURE"), shippingDocRptLineJustification.colOrdUOM)
                    .WriteLine()

                    ' 2nd line
                    If Not Trim(ds.Tables(0).Rows(I).Item("ISA_USER_DEFINED_3")) = "" Then
                        .Write("Staged Loc - " & ds.Tables(0).Rows(I).Item("ISA_USER_DEFINED_3"), shippingDocRptLineJustification.colLineSched)
                    End If

                    'If arrDescr.Count > 1 Then
                    '    For Z = 1 To arrDescr.Count - 1
                    '        .WriteLine(arrDescr(Z), shippingDocRptLineJustification.colDescr)
                    '    Next
                    'Else
                    '    If Not Trim(ds.Tables(0).Rows(I).Item("ISA_USER_DEFINED_3")) = "" Then
                    '        .WriteLine()
                    '    End If
                    'End If

                    If arrDescr.Count > 1 Then
                        .WriteLine(arrDescr(1), shippingDocRptLineJustification.colDescr)
                    Else
                        If Not Trim(ds.Tables(0).Rows(I).Item("ISA_USER_DEFINED_3")) = "" Then
                            .WriteLine()
                        End If
                    End If

                    ' 3rd line
                    .Write("PUT AWAY LOCATION:   ", shippingDocRptLineJustification.colShipCntrID)
                    If sPutawayLoc.Trim.Length > 0 Then
                        Dim myF As System.Drawing.Font = .CurrentF
                        Dim myH As Integer = .CurrentH
                        .CurrentF = arial10B
                        .CurrentH = arial10B.Height
                        .Write(sPutawayLoc, shippingDocRptLineJustification.colLineSched)
                        .CurrentF = myF
                        .CurrentH = myH
                    End If

                    If arrDescr.Count > 2 Then
                        For Z = 2 To arrDescr.Count - 1
                            .WriteLine(arrDescr(Z), shippingDocRptLineJustification.colDescr)
                        Next
                    Else
                        .WriteLine()
                    End If

                    ' after ALL item description have been printed
                    'If sPutawayLoc.Trim.Length > 0 Then
                    '    Dim myF As System.Drawing.Font = .CurrentF
                    '    Dim myH As Integer = .CurrentH
                    '    .CurrentF = arial10
                    '    .CurrentH = arial10.Height
                    '    .WriteLine(sPutawayLoc, shippingDocRptLineJustification.colShipCntrID)
                    '    .CurrentF = myF
                    '    .CurrentH = myH
                    'End If
                    If sNotice.Trim.Length > 0 Then
                        Dim myF As System.Drawing.Font = .CurrentF
                        Dim myH As Integer = .CurrentH
                        .CurrentF = arial8
                        .CurrentH = arial8.Height
                        .WriteLine(sNotice, shippingDocRptLineJustification.colShipCntrID)
                        .CurrentF = myF
                        .CurrentH = myH
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

    Private Sub DrawRectangleFloat(ByVal e As Printing.PrintPageEventArgs, _
                                   ByVal column As Integer, ByVal line As Integer, _
                                   ByVal width As Integer, ByVal Height As Integer)
        ' Create pen.
        Dim blackPen As New Pen(Color.Black, 3)
        e.Graphics.DrawRectangle(blackPen, column, line, width, Height)
    End Sub

    Private Sub GeneratePage(ByVal e As shippingDocPageEventArgs)

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

    Private Function FormatWrap(ByVal e As shippingDocPageEventArgs, _
                                ByVal currentText As String, ByVal pFont As Font, _
                                ByVal fieldsize As Integer) As ArrayList

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

        FormatWrap = arrText

    End Function

    Private Function getShipto(ByVal strShipto As String) As DataSet

        Dim rtn As String = "shippingDoc.getShipto"

        Dim sql As SQLBuilder = Nothing

        'Dim strSQLstring = "SELECT A.DESCR, A.ADDRESS1, A.ADDRESS2," & vbCrLf & _
        '            " A.ADDRESS3, A.ADDRESS4, A.CITY, A.STATE, A.POSTAL" & vbCrLf & _
        '            " FROM PS_LOCATION_TBL A" & vbCrLf & _
        '            " WHERE A.EFFDT =" & vbCrLf & _
        '            " (SELECT MAX(A_ED.EFFDT) FROM PS_LOCATION_TBL A_ED" & vbCrLf & _
        '            " WHERE A.SETID = A_ED.SETID" & vbCrLf & _
        '            " AND A.LOCATION = A_ED.LOCATION" & vbCrLf & _
        '            " AND A_ED.EFFDT <= SYSDATE)" & vbCrLf & _
        '            " AND A.LOCATION = '" & strShipto & "'" & vbCrLf & _
        '            " AND A.EFF_STATUS = 'A'"

        'Dim dbConn As String = _
        '"Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=" & mDB

        sql = New SQLBuilder(myCommon.LoadQuery(m_pathSQLs & "getLocAddress_InvTransfer_SELECT.sql"))
        sql.Parameters.Add(":KEY_LOCATION", strShipto)

        LogVerbose(msg:=rtn & "::CN string : " & Me.oraCNstring)
        LogVerbose(msg:=rtn & "::executing : " & sql.ToString)

        ''"data source=ineroth;initial catalog=pubs;integrated security=SSPI"
        'Dim da As New OleDb.OleDbDataAdapter(strSQLstring, dbConn)
        Dim da As New OleDb.OleDbDataAdapter(sql.ToString, Me.oraCNstring)
        Dim dsAddr As New DataSet
        da.Fill(dsAddr)

        sql = Nothing

        Return dsAddr

    End Function

    Private Function getTextLength(ByVal e As shippingDocPageEventArgs, _
                                   ByVal currentText As String, _
                                   ByVal pFont As Font) As Integer

        getTextLength = CInt(e.Graphics.MeasureString(currentText, pFont).Width)

    End Function

    Private Sub PrintHeader(ByVal e As shippingDocPageEventArgs)

        Dim rtn As String = "shippingDoc.PrintHeader"

        Dim field As Integer
        'Dim strSQLstring As String
        Dim intContainers As Integer
        If e.PageNumber = 1 Then
            '    Dim dbConn As String = _
            '"Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=" & mDB

            Dim sql As SQLBuilder = Nothing

            'strSQLstring = "SELECT DISTINCT A.SHIP_CNTR_ID" & vbCrLf & _
            '            " FROM PS_ISA_PICKING_CNT A, PS_ISA_PICKING_INT B" & vbCrLf & _
            '            " WHERE A.SOURCE_BUS_UNIT = '" & mSrcBU & "'" & vbCrLf & _
            '            " AND A.ORDER_NO = '" & mOrderNo & "'" & vbCrLf & _
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
            '            " AND A.RECV_LN_NBR = B.RECV_LN_NBR" & vbCrLf & _
            '            " AND NOT B.ENDDTTM IS NULL" & vbCrLf & _
            '            " AND B.QTY_PICKED > 0" & vbCrLf

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

            'strSQLstring = "SELECT (B.ISA_APPROVAL_CD || '-' || B.ISA_DESC) as PDESC" & vbCrLf & _
            '            " FROM PS_ISA_PICKING_INT A, PS_ISA_ORD_INTFC_M B" & vbCrLf & _
            '            " WHERE A.SOURCE_BUS_UNIT = '" & mSrcBU & "'" & vbCrLf & _
            '            " AND A.ORDER_NO = '" & mOrderNo & "'" & vbCrLf & _
            '            " AND A.ORDER_NO = B.ORDER_NO" & vbCrLf & _
            '            " AND A.INV_ITEM_ID = B.INV_ITEM_ID" & vbCrLf & _
            '            " AND A.ORDER_INT_LINE_NO = B.LINE_NBR" & vbCrLf & _
            '            " AND ROWNUM < 2"

            sql = New SQLBuilder(myCommon.LoadQuery(m_pathSQLs & "getApprover_InvTransfer_SELECT.sql"))
            sql.Parameters.Add(":KEY_SOURCE_BU", mSrcBU)
            sql.Parameters.Add(":KEY_ORDER_NO", mOrderNo)

            LogVerbose(msg:=rtn & "::CN string : " & Me.oraCNstring)
            LogVerbose(msg:=rtn & "::executing : " & sql.ToString)

            'Dim dbConn2 As New System.Data.OleDb.OleDbConnection(dbConn)
            Dim dbConn2 As New System.Data.OleDb.OleDbConnection(Me.oraCNstring)
            dbConn2.Open()

            Dim dr2 As New OleDb.OleDbCommand(sql.ToString, dbConn2)
            strPriority = dr2.ExecuteScalar

            Try
                If Not (strPriority Is Nothing) Then
                    If (strPriority.Trim.Length = 0) Then
                        strPriority = "___"
                    End If
                Else
                    strPriority = "___"
                End If
            Catch ex As Exception
            End Try

            sql = Nothing
        End If

        With e

            .CurrentF = arial12B
            .CurrentH = arial12B.Height
            .Write(("Priority: " & strPriority), shippingDocRptLineJustification.Right)
            .CurrentF = arial20B
            .CurrentH = arial20B.Height
            .WriteLine("SDI, Inc. ", shippingDocRptLineJustification.Centered)
            If e.PageNumber = 1 Then
                .Write("SHIPPING DOCUMENT", shippingDocRptLineJustification.Centered)
                .CurrentF = arial12
                .CurrentH = arial12.Height
                .Write("Total Containers = " & intContainers, shippingDocRptLineJustification.Right)
                .CurrentF = arial20B
                .CurrentH = arial20B.Height
                .WriteLine()
            Else
                .WriteLine("SHIPPING DOCUMENT", shippingDocRptLineJustification.Centered)
            End If


            ' display a horizontal line to separate the header from
            ' the body
            .HorizontalRule(1)
        End With

    End Sub

    Private Sub PrintFooter(ByVal e As shippingDocPageEventArgs)

        Dim ver As String = ""

        Try
            ver = "v" & System.Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString
        Catch ex As Exception
        End Try

        With e

            If mRow >= ds.Tables(0).Rows.Count Then
                If .CurrentY + 70 > e.PageBottom Then
                    e.HasMorePages = True
                Else
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
                .Write("* indicates item is in multiple packages.", shippingDocRptLineJustification.Centered)
            End If

            ' write the right-side footer text
            If Len(mFooterRight) > 0 Then
                .WriteLine(mFooterRight)
            Else
                ' we default to displaying the current page number
                .WriteLine("Page " & e.PageNumber, shippingDocRptLineJustification.Right)
            End If
            If ver.Length > 0 Then
                .WriteLine(ver, shippingDocRptLineJustification.Right)
            End If
            If e.PageNumber = 1 Then
                PrintDocumentOnPrintPageFromDLL(e, mOrderNo)
            End If

        End With

    End Sub

    Private Sub buildDatasets()

        Dim rtn As String = "shippingDoc.buildDatasets"

        'Dim strSQLstring As String
        'strSQLstring = "SELECT A.ORDER_NO," & vbCrLf & _
        '            " A.ORDER_INT_LINE_NO, A.SCHED_LINE_NO," & vbCrLf & _
        '            " A.INV_ITEM_ID, B.SHIP_CNTR_ID, L.ISA_WORK_ORDER_NO," & vbCrLf & _
        '            " (E.DESCR254 ||' MFG: '||M.MFG_ID||'/'||M.MFG_ITM_ID) as DESCR254," & vbCrLf & _
        '            " TO_CHAR(A.ISA_PACK_DTTM,'YYYY-MM-DD') as pick_date," & vbCrLf & _
        '            " A.ISA_USER_DEFINED_3," & vbCrLf & _
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
        '            " C.ADD_DTTM, L.ISA_EMPLOYEE_ID, G.ISA_EMPLOYEE_NAME, A.ISA_USER_DEFINED_3" & vbCrLf & _
        '            " UNION ALL" & vbCrLf & _
        '            " SELECT A.ORDER_NO," & vbCrLf & _
        '            " A.ORDER_INT_LINE_NO, A.SCHED_LINE_NO," & vbCrLf & _
        '            " A.INV_ITEM_ID, B.SHIP_CNTR_ID, L.ISA_WORK_ORDER_NO," & vbCrLf & _
        '            " (E.DESCR254 ||' MFG: '||M.MFG_ID||'/'||M.MFG_ITM_ID) as DESCR254," & vbCrLf & _
        '            " TO_CHAR(A.ISA_PACK_DTTM,'YYYY-MM-DD') as pick_date," & vbCrLf & _
        '            " A.ISA_USER_DEFINED_3," & vbCrLf & _
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
        '            " C.ADD_DTTM, L.ISA_EMPLOYEE_ID, G.ISA_EMPLOYEE_NAME, A.ISA_USER_DEFINED_3" & vbCrLf & _
        '            " ORDER BY SHIP_CNTR_ID, ORDER_INT_LINE_NO,  SCHED_LINE_NO"

        '' A.DEMAND_LINE_NO, was removed from the group by

        ''" AND A.ORDER_INT_LINE_NO = C.LINE_NBR" & vbCrLf & _
        ''            " AND B.ISA_IDENTIFIER = D.ISA_PARENT_IDENT" & vbCrLf & _
        ''            " AND A.ORDER_INT_LINE_NO = D.LINE_NBR" & vbCrLf & _
        ''            " AND E.INV_ITEM_ID = D.INV_ITEM_ID" & vbCrLf & _
        ''            " ORDER BY A.SHIP_CNTR_ID, A.ORDER_INT_LINE_NO,  A.SCHED_LINE_NO"

        ''" ORDER BY A.SHIP_CNTR_ID, A.ORDER_INT_LINE_NO,  A.SCHED_LINE_NO"

        ''Dim dbConn As String = _
        ''"Provider=MSDAORA.1;Password=einternet;User ID=einternet;Data Source=" & mDB

        Dim sql As SQLBuilder = Nothing

        If Not m_bIsIgnoreShipDTTM Then
            ' if overriden, query will not consider SHIP_DTTM to be NULL
            sql = New SQLBuilder(myCommon.LoadQuery(m_pathSQLs & "shippingDoc_InvTransfer_SELECT_ignoreShipDT.sql"))
        Else
            ' user regular - SHIP_DTTM should be NULL
            sql = New SQLBuilder(myCommon.LoadQuery(m_pathSQLs & "shippingDoc_InvTransfer_SELECT.sql"))
        End If
        sql.Parameters.Add(":KEY_SOURCE_BU", mSrcBU)
        sql.Parameters.Add(":KEY_ORDER_NO", mOrderNo)

        LogVerbose(msg:=rtn & "::CN string : " & Me.oraCNstring)
        LogVerbose(msg:=rtn & "::executing : " & sql.ToString)

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

        sql = New SQLBuilder(myCommon.LoadQuery(m_pathSQLs & "getShipTo_InvTransfer_SELECT.sql"))
        sql.Parameters.Add(":KEY_SOURCE_BU", mSrcBU)
        sql.Parameters.Add(":KEY_ORDER_NO", mOrderNo)

        LogVerbose(msg:=rtn & "::CN string : " & Me.oraCNstring)
        LogVerbose(msg:=rtn & "::executing : " & sql.ToString)

        'Dim da1 As New OleDb.OleDbDataAdapter(strSQLstring, dbConn)
        Dim da1 As New OleDb.OleDbDataAdapter(sql.ToString, Me.oraCNstring)

        da1.Fill(ds1)
        If ds1.Tables(0).Rows.Count > 0 Then
            ds2 = getShipto(ds1.Tables(0).Rows(0).Item("SHIPTO_ID"))
        Else
            ds2 = getShipto(" ")
        End If

        sql = Nothing
    End Sub

    Private Sub PrintDocumentOnPrintPageFromDLL(ByVal e As PrintPageEventArgs, _
                                                ByVal strOrderNO As String)
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
