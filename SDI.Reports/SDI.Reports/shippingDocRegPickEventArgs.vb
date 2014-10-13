Imports System.Drawing
Imports System.Drawing.Printing

''' <summary>
''' This is a list of the possible text justification values
''' used by the 
''' <see cref="M:vbReport.ReportPageEventArgs1.Write(System.String,vbReport.shippingDocRegPickLineJustification)" />
''' and
''' <see cref="M:vbReport.ReportPageEventArgs1.WriteLine(System.String,vbReport.shippingDocRegPickLineJustification)" />
''' methods.
''' </summary>
Public Enum shippingDocRegPickLineJustification
    Left = 0
    Centered = 1
    Right = 2
    col1 = 3
    col2 = 4
    col3 = 5
    col4 = 6
    colShipCntrID = 7
    colLineSched = 9
    colDemSrc = 10
    colShipDate = 11
    colItemID = 13
    colDescr = 14
    colQtyOrdered = 15
    colQtyShipped = 16
    colOrdUOM = 17
    colShipUOM = 18

End Enum


''' <summary>
''' The ReportPageEventArgs1 the type of the parameter provided by
''' the events raised from the <see cref="T:vbReport.ReportDocument1" /> 
''' object. This class includes methods to simplify the process of
''' rendering text output into each page of the report.
''' </summary>
Public Class shippingDocRegPickEventArgs

    Inherits PrintPageEventArgs

    Private mFont As Font
    Private mBrush As Brush
    Private mPageNumber As Integer
    Private mX As Integer
    Private mY As Integer
    Private mY2 As Integer
    Private mFooterLines As Integer
    Private mLineHeight As Integer
    Private mShadingHeight As Integer
    Private mPageBottom As Integer

    Friend Sub New(ByVal e As PrintPageEventArgs, _
        ByVal pageNumber As Integer, ByVal font As Font, _
        ByVal brush As Brush, ByVal footerLines As Integer)

        MyBase.New(e.Graphics, e.MarginBounds, e.PageBounds, e.PageSettings)
        mPageNumber = pageNumber
        mFont = font
        mBrush = brush
        PositionToStart()
        mFooterLines = footerLines

        mLineHeight = CInt(mFont.GetHeight(Graphics))
        mShadingHeight = 45

        mPageBottom = MarginBounds.Bottom - mFooterLines * mLineHeight - mLineHeight

    End Sub

    ''' <summary>
    ''' Writes some text to the report starting at the current cursor location.
    ''' The cursor is moved to the right, but not down to the next line.
    ''' </summary>
    ''' <param name="Text">The text to render.</param>
    Public Sub Write(ByVal Text As String)

        Graphics.DrawString(Text, mFont, mBrush, mX, mY)
        mX += CInt(Graphics.MeasureString(Text, mFont).Width)

    End Sub

    ''' <summary>
    ''' Writes some text to the report starting at the current cursor location.
    ''' The cursor is moved to the right, but not down to the next line.
    ''' </summary>
    ''' <param name="Text">The text to render.</param>
    Public Sub Write(ByVal Text As String, ByVal mFont As Font)

        Graphics.DrawString(Text, mFont, mBrush, mX, mY)
        mX += CInt(Graphics.MeasureString(Text, mFont).Width)

    End Sub

    ''' <summary>
    ''' Writes some text to the report starting at the current cursor location.
    ''' The cursor is moved to the right, but not down to the next line.
    ''' </summary>
    ''' <param name="Text">The text to render.</param>
    Public Sub Writeh(ByVal Text As String, ByVal hFont As System.Drawing.Font)

        Graphics.DrawString(Text, hFont, mBrush, mX, mY)
        mX += CInt(Graphics.MeasureString(Text, hFont).Width)

    End Sub

    ''' <summary>
    ''' Writes text to the report on the current line, but justified based on
    ''' the justification parameter value. 
    ''' The cursor is moved to the right, but not down to the next line.
    ''' </summary>
    ''' <param name="Text">The text to render.</param>
    ''' <param name="Justification">Indicates the justification for the text.</param>
    Public Sub Write(ByVal Text As String, _
        ByVal Justification As shippingDocRegPickLineJustification)

        mX = GetMX(Text, Justification)
        Write(Text)

    End Sub

    Public Sub Write(ByVal Text As Decimal, _
        ByVal Justification As shippingDocRegPickLineJustification)

        mX = GetMX(Text, Justification)
        Write(Text)

    End Sub

    ''' <summary>
    ''' Writes text to the report on the current line, but justified based on
    ''' the justification parameter value. 
    ''' The cursor is moved to the right, but not down to the next line.
    ''' </summary>
    ''' <param name="Text">The text to render.</param>
    ''' <param name="Justification">Indicates the justification for the text.</param>
    Public Sub Write(ByVal Text As String, _
        ByVal mX1 As Integer)

        mX = mX1
        Write(Text)

    End Sub

    ''' <summary>
    ''' Moves the cursor down one line and to the left side of the page.
    ''' </summary>
    Public Sub WriteLine()

        mX = MarginBounds.Left
        mY += mLineHeight

    End Sub

    ''' <summary>
    ''' Writes text to the report starting at the current cursor location and 
    ''' then moves the cursor down one line and to the left side of the page.
    ''' </summary>
    ''' <param name="Text">The text to render.</param>
    Public Sub WriteLine(ByVal Text As String)

        Graphics.DrawString(Text, mFont, mBrush, mX, mY)
        WriteLine()

    End Sub

    ''' <summary>
    ''' Writes text to the report on the current line, but justified based on
    ''' the justification parameter value. 
    ''' The cursor is moved to the right, but not down to the next line.
    ''' </summary>
    ''' <param name="Text">The text to render.</param>
    ''' <param name="Justification">Indicates the justification for the text.</param>
    Public Sub WriteLine(ByVal Text As String, _
        ByVal mX1 As Integer)

        mX = mX1
        Write(Text)
        WriteLine()

    End Sub

    Public Sub WriteLine(ByVal Text As String, _
        ByVal Justification As shippingDocRegPickLineJustification)

        mX = GetMX(Text, Justification)
        Write(Text)
        WriteLine()

    End Sub

    Private Function GetMX(ByVal Text As String, _
        ByVal Justification As shippingDocRegPickLineJustification) As Integer

        Select Case Justification
            Case shippingDocRegPickLineJustification.Left
                GetMX = MarginBounds.Left

            Case shippingDocRegPickLineJustification.Centered
                GetMX = MarginBounds.Left + CInt(MarginBounds.Width / 2 - _
                  Graphics.MeasureString(Text, mFont).Width / 2)

            Case shippingDocRegPickLineJustification.Right
                GetMX = CInt(MarginBounds.Right - Graphics.MeasureString(Text, mFont).Width)

            Case shippingDocRegPickLineJustification.col1
                GetMX = MarginBounds.Left

            Case shippingDocRegPickLineJustification.col2
                GetMX = 255

            Case shippingDocRegPickLineJustification.col3
                GetMX = 625

            Case shippingDocRegPickLineJustification.col4
                GetMX = 715

            Case shippingDocRegPickLineJustification.colShipCntrID
                GetMX = 100

            Case shippingDocRegPickLineJustification.colLineSched
                GetMX = 259

            Case shippingDocRegPickLineJustification.colDemSrc
                GetMX = 329

            Case shippingDocRegPickLineJustification.colShipDate
                GetMX = 366

            Case shippingDocRegPickLineJustification.colItemID
                ' GetMX = 444 +13
                GetMX = 457

            Case shippingDocRegPickLineJustification.colDescr
                'GetMX = 557 +13
                GetMX = 570

            Case shippingDocRegPickLineJustification.colQtyOrdered
                GetMX = 825

            Case shippingDocRegPickLineJustification.colQtyShipped
                GetMX = 895

            Case shippingDocRegPickLineJustification.colOrdUOM
                GetMX = 965


        End Select

    End Function

    ''' <summary>
    ''' Draws a horizontal line across the width of the page on the current
    ''' line. After the line is drawn the cursor is moved down one line and
    ''' to the left side of the page.
    ''' </summary>
    Public Sub HorizontalRule(ByVal pencolor As Integer)

        Dim y As Integer = mY + CInt(mLineHeight / 2)

        Select Case pencolor
            Case 1
                Graphics.DrawLine(Pens.Black, MarginBounds.Left, y, MarginBounds.Right, y)
            Case 2
                Graphics.DrawLine(Pens.LightGray, MarginBounds.Left, y, MarginBounds.Right, y)
        End Select

        WriteLine()


    End Sub

    ''' <summary>
    ''' Draws a horizontal shaded line across the width of the page on the current
    ''' line. 
    ''' </summary>
    Public Sub HorizontalShading()

        Graphics.FillRectangle(New SolidBrush(Color.LightGray), (MarginBounds.Left + 3), (mY - 5), 645, 45)

    End Sub

    ''' <summary>
    ''' Sets or returns the current X position (left to right) of the
    ''' cursor on the page.
    ''' </summary>
    ''' <value>The horizontal position of the cursor.</value>
    Public Property CurrentX() As Integer
        Get
            Return mX
        End Get
        Set(ByVal Value As Integer)
            mX = Value
        End Set
    End Property

    ''' <summary>
    ''' Sets or returns the current Y position (top to bottom) of the
    ''' cursor on the page.
    ''' </summary>
    ''' <value>The vertical position of the cursor.</value>
    Public Property CurrentY() As Integer
        Get
            Return mY
        End Get
        Set(ByVal Value As Integer)
            mY = Value
        End Set
    End Property

    Public Property CurrentS() As Integer
        Get
            Return mShadingHeight
        End Get
        Set(ByVal Value As Integer)
            mShadingHeight = Value
        End Set
    End Property

    ''' <summary>
    ''' Sets or returns the current Y position (top to bottom) of the
    ''' cursor on the page.
    ''' </summary>
    ''' <value>The vertical position of the cursor.</value>
    Public Property CurrentY2() As Integer
        Get
            Return mY2
        End Get
        Set(ByVal Value As Integer)
            mY2 = Value
        End Set
    End Property

    ''' <summary>
    ''' Sets or returns the current font of the
    ''' page.
    ''' </summary>
    ''' <value>current font</value>
    Public Property CurrentF() As Font
        Get
            Return mFont
        End Get
        Set(ByVal Value As Font)
            mFont = Value
        End Set
    End Property

    ''' <summary>
    ''' Sets or returns the current font height of the
    ''' page.
    ''' </summary>
    ''' <value>current font</value>
    Public Property CurrentH() As Integer
        Get
            Return mLineHeight
        End Get
        Set(ByVal Value As Integer)
            mLineHeight = Value
        End Set
    End Property

    ''' <summary>
    ''' Moves the cursor to the top left corner of the page.
    ''' </summary>
    Public Sub PositionToStart()

        mX = MarginBounds.Left
        mY = MarginBounds.Top

    End Sub

    ''' <summary>
    ''' Returns the Y value correspondign to the bottom of the page
    ''' body. This is the position immediately above the start of the 
    ''' page footer.
    ''' </summary>
    ''' <value>The Y value of the bottom of the page.</value>
    Public ReadOnly Property PageBottom() As Integer
        Get
            Return mPageBottom + mLineHeight
        End Get
    End Property

    ''' <summary>
    ''' Returns True if the cursor's current location is beyond the bottom of
    ''' the page body. This doesn't mean we're into the bottom margin, but may
    ''' indicate that the cursor in the page's footer region.
    ''' </summary>
    ''' <value>A Boolean indicating whether the cursor is past the end of the page.</value>
    Public ReadOnly Property EndOfPage() As Boolean
        Get
            Return mY >= mPageBottom
        End Get
    End Property

    ''' <summary>
    ''' Returns the page number of the current page. This value is automatically
    ''' incremented as each new page is rendered.
    ''' </summary>
    ''' <value>The current page number.</value>
    Public ReadOnly Property PageNumber() As Integer
        Get
            Return mPageNumber
        End Get
    End Property

End Class
