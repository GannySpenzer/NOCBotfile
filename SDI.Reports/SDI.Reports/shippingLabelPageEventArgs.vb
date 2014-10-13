Option Strict On

Imports System.Drawing
Imports System.Drawing.Printing

''' <summary>
''' This is a list of the possible text justification values
''' used by the 
''' <see cref="M:vbReport.ReportPageEventArgs1.Write(System.String,vbReport.shippingLabelRptLineJustification)" />
''' and
''' <see cref="M:vbReport.ReportPageEventArgs1.WriteLine(System.String,vbReport.shippingLabelRptLineJustification)" />
''' methods.
''' </summary>
'Public Enum ReportLineJustification
Public Enum shippingLabelRptLineJustification
    Left = 0
    Centered = 1
    Right = 2
    col1 = 3
    col2 = 4
    col3 = 5
    col4 = 6
    col5 = 14
    col6 = 21
    col7 = 22
    col8 = 23
    colShipCntrID = 7
    colProductNumber = 8
    colLineSched = 9
    colUOM = 19
    colDemSrc = 10
    colShipDate = 11
    colItemID = 13
    colDescr = 14
    colQtyOrdered = 15
    colQtyShipped = 16
    colOrdUOM = 17
    colShipUOM = 18
    colQtyBackorder = 20
End Enum


''' <summary>
''' The ReportPageEventArgs1 the type of the parameter provided by
''' the events raised from the <see cref="T:vbReport.ReportDocument1" /> 
''' object. This class includes methods to simplify the process of
''' rendering text output into each page of the report.
''' </summary>

Public Class shippingLabelPageEventArgs

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

    Public Sub RotatePrint()

        ' Reset the transformation.
        Graphics.ResetTransform()
        ' Rotate.
        Graphics.RotateTransform(180, System.Drawing.Drawing2D.MatrixOrder.Append)

        Graphics.TranslateTransform(1065, 818, System.Drawing.Drawing2D.MatrixOrder.Append)

    End Sub

    ''' <summary>
    ''' Writes some text to the report starting at the current cursor location.
    ''' The cursor is moved to the right, but not down to the next line.
    ''' </summary>
    ''' <param name="Text">The text to render.</param>
    Public Sub Write(ByVal Text As String)

        ' Transform the point (0, 0) to see where it goes.
        'Dim pts() As Point = {New Point(0, 0)}
        'Graphics.Transform.TransformPoints(pts)

        ' Make a transformation that draws rotated text
        ' translated to this point.
        ' Reset the transformation.
        Graphics.ResetTransform()
        ' Rotate.
        Graphics.RotateTransform(180, System.Drawing.Drawing2D.MatrixOrder.Append)
        ' Translate to the target point's transformed location.
        'Graphics.TranslateTransform(pts(0).X, pts(0).Y)

        'Dim angleRadian As Double = ((180 Mod 360) / 180) * Math.PI
        'Graphics.TranslateTransform(Graphics.MeasureString(Text, mFont).Width + CInt(Math.Sin(angleRadian) * Graphics.MeasureString(Text, mFont).Height), Graphics.MeasureString(Text, mFont).Height)

        Graphics.TranslateTransform(1067, 818, System.Drawing.Drawing2D.MatrixOrder.Append)

        'Graphics.RotateTransform(180)
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
        'Graphics.RotateTransform(180)
        mX += CInt(Graphics.MeasureString(Text, mFont).Width)

    End Sub

    ''' <summary>
    ''' Writes some text to the report starting at the current cursor location.
    ''' The cursor is moved to the right, but not down to the next line.
    ''' </summary>
    ''' <param name="Text">The text to render.</param>
    Public Sub Writeh(ByVal Text As String, ByVal hFont As System.Drawing.Font)

        Graphics.DrawString(Text, hFont, mBrush, mX, mY)
        'Graphics.RotateTransform(180)

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
                     ByVal Justification As shippingLabelRptLineJustification)

        mX = GetMX(Text, Justification)
        Write(Text)

    End Sub

    Public Sub Write(ByVal Text As Decimal, _
                     ByVal Justification As shippingLabelRptLineJustification)

        mX = GetMX(CStr(Text), Justification)
        Write(CStr(Text))

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
        'Graphics.RotateTransform(180)

        WriteLine()

    End Sub

    ''' <summary>
    ''' Writes text to the report on the current line, but justified based on
    ''' the justification parameter value. 
    ''' The cursor is moved to the right, but not down to the next line.
    ''' </summary>
    ''' <param name="Text">The text to render.</param>
    ''' <param name="Justification">Indicates the justification for the text.</param>
    'Public Sub WriteLine(ByVal Text As String, _
    '    ByVal mX1 As Integer)

    '    mX = mX1
    '    Write(Text)
    '    WriteLine()

    'End Sub

    Public Sub WriteLine(ByVal Text As String, _
                         ByVal Justification As shippingLabelRptLineJustification)


        mX = GetMX(Text, Justification)
        Write(Text)
        WriteLine()

    End Sub

    Private Function GetMX(ByVal Text As String, _
                           ByVal Justification As shippingLabelRptLineJustification) As Integer

        Select Case Justification
            Case shippingLabelRptLineJustification.Left
                GetMX = MarginBounds.Left

            Case shippingLabelRptLineJustification.Centered
                GetMX = MarginBounds.Left + CInt(MarginBounds.Width / 2 - _
                  Graphics.MeasureString(Text, mFont).Width / 2)

            Case shippingLabelRptLineJustification.Right
                GetMX = CInt(MarginBounds.Right - Graphics.MeasureString(Text, mFont).Width)

            Case shippingLabelRptLineJustification.col1
                GetMX = MarginBounds.Left

            Case shippingLabelRptLineJustification.col2
                GetMX = 135

            Case shippingLabelRptLineJustification.col3
                GetMX = 275

            Case shippingLabelRptLineJustification.col4
                GetMX = 375

            Case shippingLabelRptLineJustification.col5
                GetMX = 550

            Case shippingLabelRptLineJustification.col6
                GetMX = 680

            Case shippingLabelRptLineJustification.col7
                GetMX = 770

            Case shippingLabelRptLineJustification.col8
                GetMX = 815

            Case shippingLabelRptLineJustification.colLineSched
                GetMX = 12

            Case shippingLabelRptLineJustification.colProductNumber
                GetMX = 102

            Case shippingLabelRptLineJustification.colShipDate
                GetMX = 246

            Case shippingLabelRptLineJustification.colUOM
                GetMX = 345

            Case shippingLabelRptLineJustification.colDescr
                GetMX = 102

            Case shippingLabelRptLineJustification.colQtyOrdered
                GetMX = 425

            Case shippingLabelRptLineJustification.colQtyShipped
                GetMX = 500

            Case shippingLabelRptLineJustification.colQtyBackorder
                GetMX = 575

        End Select

    End Function

    ''' <summary>
    ''' Draws a horizontal line across the width of the page on the current
    ''' line. After the line is drawn the cursor is moved down one line and
    ''' to the left side of the page.
    ''' </summary>
    Public Sub HorizontalRule()

        Dim y As Integer = mY + CInt(mLineHeight / 2)

        Graphics.DrawLine(Pens.Black, MarginBounds.Left, y, 658, y)
        WriteLine()

    End Sub
    ''' <summary>
    ''' Draws a horizontal shaded line across the width of the page on the current
    ''' line. 
    ''' </summary>
    Public Sub HorizontalShading(ByVal intStartMY As Integer, ByVal intLineCnt As Integer)
        Dim intHalfLine As Integer = CInt(mLineHeight / 2)
        Graphics.FillRectangle(New SolidBrush(Color.LightGray), (MarginBounds.Left + 3), (intStartMY - 5), 645, ((mLineHeight * (intLineCnt + 1)) + intHalfLine))
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
