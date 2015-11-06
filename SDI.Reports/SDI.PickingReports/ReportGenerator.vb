Public Class ReportGenerator

    Implements SDI.Reports.ISDiReport

    ' shipping document (Transfers)
    Public Const RPT_SHIPPING_DOCUMENT_PRINT_TO As String = "SDI.Reports.shippingDoc.printTo"
    Public Const RPT_SHIPPING_DOCUMENT_PAPERSOURCE As String = "SDI.Reports.shippingDoc.paperSourceId"
    Public Const RPT_SHIPPING_DOCUMENT_COPY As String = "SDI.Reports.shippingDoc.copy"
    ' shipping document (Regular)
    Public Const RPT_SHIPPING_DOCUMENT_REG_PICK_PRINT_TO As String = "SDI.Reports.shippingDocRegPick.printTo"
    Public Const RPT_SHIPPING_DOCUMENT_REG_PICK_PAPERSOURCE As String = "SDI.Reports.shippingDocRegPick.paperSourceId"
    Public Const RPT_SHIPPING_DOCUMENT_REG_PICK_COPY As String = "SDI.Reports.shippingDocRegPick.copy"

    ' shipping label / packing slip (Transfer)
    Public Const RPT_SHIPPING_LABEL_PRINT_TO As String = "SDI.Reports.shippingLabel.printTo"
    Public Const RPT_SHIPPING_LABEL_PAPERSOURCE As String = "SDI.Reports.shippingLabel.paperSourceId"
    Public Const RPT_SHIPPING_LABEL_COPY As String = "SDI.Reports.shippingLabel.copy"
    ' shipping label / packing slip (Regular)
    Public Const RPT_SHIPPING_LABEL_REG_PICK_PRINT_TO As String = "SDI.Reports.shippingLabelRegPick.printTo"
    Public Const RPT_SHIPPING_LABEL_REG_PICK_PAPERSOURCE As String = "SDI.Reports.shippingLabelRegPick.paperSourceId"
    Public Const RPT_SHIPPING_LABEL_REG_PICK_COPY As String = "SDI.Reports.shippingLabelRegPick.copy"

    Public Const RPT_PARTIAL_PICK_EXCLUDE_STATE As String = "SDI.Reports.PartialPickExcludeState"

    Public Const DEBUG_LIST_AVAILABLE_PRINTERS As String = "debug.ListAvailPrinter"
    Public Const DEBUG_LIST_AVAIL_PRN_PAPERSOURCES As String = "debug.ListAvailPrnPaperSources"

    Public Const USER_OVERRIDE_USER_ID As String = "userOverride.userId"
    Public Const USER_OVERRIDE_PRINT_TO As String = "userOverride.printTo"
    Public Const USER_OVERRIDE_SHIPPING_DOCUMENT_REG_PICK_PAPERSOURCE As String = "userOverride.shippingDocRegPick.paperSourceId"
    Public Const USER_OVERRIDE_SHIPPING_LABEL_REG_PICK_PAPERSOURCE As String = "userOverride.shippingLabelRegPick.paperSourceId"
    Public Const USER_OVERRIDE_SHIPPING_DOCUMENT_PAPERSOURCE As String = "userOverride.shippingDoc.paperSourceId"
    Public Const USER_OVERRIDE_SHIPPING_LABEL_PAPERSOURCE As String = "userOverride.shippingLabel.paperSourceId"

    Public Const BU_INVENTORY_OVERRIDE_ID As String = "buInventoryOverride.BUs"
    Public Const BU_INVENTORY_OVERRIDE_PRINT_TO As String = "buInventoryOverride.printTo"
    Public Const BU_INVENTORY_OVERRIDE_SHIPPING_DOCUMENT_REG_PICK_PAPERSOURCE As String = "buInventoryOverride.shippingDocRegPick.paperSourceId"
    Public Const BU_INVENTORY_OVERRIDE_SHIPPING_LABEL_REG_PICK_PAPERSOURCE As String = "buInventoryOverride.shippingLabelRegPick.paperSourceId"
    Public Const BU_INVENTORY_OVERRIDE_SHIPPING_DOCUMENT_PAPERSOURCE As String = "buInventoryOverride.shippingDoc.paperSourceId"
    Public Const BU_INVENTORY_OVERRIDE_SHIPPING_LABEL_PAPERSOURCE As String = "buInventoryOverride.shippingLabel.paperSourceId"

    '' printer
    'Public Const PRN_PRINT_TO_OVERRIDE As String = "printToOverride"

    Public Enum ePickingReportType As Integer
        unknown = 0
        shippingDocument = 1
        packingSlip = 2
        shippingDocRegPick = 3
        packingSlipRegPick = 4
    End Enum

    Public Event AssignedPrintedPickedOrderLines(ByVal sender As Object, ByVal e As EventArgs) Implements SDI.reports.ISDiReport.AssignedPrintedPickedOrderLines

    Private Const oraCN_default_provider As String = "Provider=MSDAORA.1;"
    Private Const oraCN_default_creden As String = "User ID=einternet;Password=einternet;"
    Private Const oraCN_default_DB As String = "Data Source=RPTG"

    Private m_bu As String = ""
    Private m_orderNo As String = ""
    Private m_logger As SDI.ApplicationLogger.IApplicationLogger = New SDI.ApplicationLogger.noAppLogger
    Private m_logLevel As System.Diagnostics.TraceLevel = TraceLevel.Off
    Private m_oraCNstring As String = "" & _
                                      oraCN_default_provider & _
                                      oraCN_default_creden & _
                                      oraCN_default_DB & _
                                      ""
    Private m_oraCN As OleDb.OleDbConnection = Nothing
    Private m_executionPath As String = ""

    Private m_userId As String = ""

    Public Sub New()

    End Sub

    Public Property BusinessUnit() As String
        Get
            Return m_bu
        End Get
        Set(ByVal Value As String)
            m_bu = Value
        End Set
    End Property

    Private m_buInv As String = ""

    Public Property BusinessUnitINV() As String
        Get
            Return m_buInv
        End Get
        Set(ByVal Value As String)
            m_buInv = Value
        End Set
    End Property

    Public Property OrderNo() As String
        Get
            Return m_orderNo
        End Get
        Set(ByVal Value As String)
            m_orderNo = Value
        End Set
    End Property

    Public Property oraCNstring() As String
        Get
            Return m_oraCNstring
        End Get
        Set(ByVal Value As String)
            m_oraCNstring = Value
            m_oraCN = New OleDb.OleDbConnection(m_oraCNstring)
        End Set
    End Property

    Public Property ExecutionPath() As String
        Get
            Return m_executionPath
        End Get
        Set(ByVal Value As String)
            m_executionPath = Value
        End Set
    End Property

    Public Property Logger() As SDI.ApplicationLogger.IApplicationLogger
        Get
            If (m_logger Is Nothing) Then
                m_logger = New SDI.ApplicationLogger.appLogger
            End If
            Return m_logger
        End Get
        Set(ByVal Value As SDI.ApplicationLogger.IApplicationLogger)
            m_logger = Value
        End Set
    End Property

    Public Property UserId() As String
        Get
            Return m_userId
        End Get
        Set(ByVal Value As String)
            m_userId = Value
        End Set
    End Property

    Private m_printCopies As Integer = 0

    Public Property PrintCopies() As Integer
        Get
            Return m_printCopies
        End Get
        Set(ByVal Value As Integer)
            m_printCopies = Value
        End Set
    End Property

    Public Sub SetLogLevel(ByVal lvl As System.Diagnostics.TraceLevel)
        m_logLevel = lvl
    End Sub

    'Public Property PrinterName() As String
    '    Get
    '        Return m_printerName
    '    End Get
    '    Set(ByVal Value As String)
    '        m_printerName = Value
    '    End Set
    'End Property

    Public Overloads Sub GenerateReports(ByVal rptTypes As ReportGenerator.ePickingReportType())
        runReports(rptTypes)
    End Sub

    Public Overloads Sub GenerateReports(ByVal rptType As ReportGenerator.ePickingReportType)
        runReports(New ReportGenerator.ePickingReportType() {rptType})
    End Sub

    Private Sub runReports(ByVal rptTypes As ReportGenerator.ePickingReportType())

        Dim rtn As String = "ReportGenerator.runReports"

        If rptTypes.Length > 0 Then
            ' get the site printer from enterprise table
            Dim sitePrnName As String = GetPrinterNameForSite(Me.BusinessUnit, m_oraCN)

            ' check/load override settings
            Dim cfgSite As siteConfig = CheckLoadOverrides(bu3:=Me.BusinessUnit.Substring(2, 3))

            '' for debugging purposes ONLY
            ''   module will ignore errors and such ... just some additional logging for printer configs
            'ListAvailPrintersAndPaperSources(cfgSite)

            ' generate reports
            For Each typ As ReportGenerator.ePickingReportType In rptTypes
                Select Case typ
                    Case ePickingReportType.packingSlip
                        Dim rpt As New SDI.Reports.shippingLabel
                        rpt.ExecutionPath = Me.ExecutionPath
                        LogVerbose(msg:=rtn & "::BUSINESS_UNIT = " & Me.BusinessUnit)
                        LogVerbose(msg:=rtn & "::BUSINESS_UNIT (INV) = " & Me.BusinessUnitINV)
                        rpt.SourceBusinessUnit = Me.BusinessUnit
                        LogVerbose(msg:=rtn & "::ORDER_NO = " & Me.OrderNo)
                        rpt.OrderNo = Me.OrderNo
                        LogVerbose(msg:=rtn & "::CN string = " & Me.oraCNstring)
                        rpt.oraCNstring = Me.oraCNstring
                        LogVerbose(msg:=rtn & "::PRINTER NAME = " & sitePrnName)
                        If (Me.PrintCopies > 0) Then
                            rpt.PrinterSettings.Copies = Me.PrintCopies
                        End If
                        Dim paperSrc As String = ""
                        Try
                            paperSrc = rpt.DefaultPageSettings.PaperSource.SourceName
                        Catch ex As Exception
                        End Try
                        LogVerbose(msg:=rtn & "::PAPER SOURCE INDEX = " & paperSrc)
                        rpt.PrinterSettings.PrinterName = sitePrnName
                        ' check/override with this printer name
                        If cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_LABEL_PRINT_TO) Then
                            LogVerbose(msg:=rtn & "::(override) PRINTER NAME = " & CStr(cfgSite.KeyValue(Me.RPT_SHIPPING_LABEL_PRINT_TO)))
                            rpt.PrinterSettings.PrinterName = CStr(cfgSite.KeyValue(Me.RPT_SHIPPING_LABEL_PRINT_TO))
                            ' check/override paper source Id, since printer was overridden
                            If cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_LABEL_PAPERSOURCE) Then
                                LogVerbose(msg:=rtn & "::(override) PAPER SOURCE INDEX = " & cfgSite.KeyValue(Me.RPT_SHIPPING_LABEL_PAPERSOURCE))
                                rpt.DefaultPageSettings.PaperSource = rpt.PrinterSettings.PaperSources(CInt(cfgSite.KeyValue(Me.RPT_SHIPPING_LABEL_PAPERSOURCE)))
                            End If
                        End If
                        ' check/override # of copies - site + print job type
                        If cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_LABEL_COPY) Then
                            If IsNumeric(cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_LABEL_COPY)) Then
                                LogVerbose(msg:=rtn & "::(override) COPIES = " & cfgSite.KeyValue(Me.RPT_SHIPPING_LABEL_COPY))
                                rpt.PrinterSettings.Copies = CInt(cfgSite.KeyValue(Me.RPT_SHIPPING_LABEL_COPY))
                            End If
                        End If
                        ' logger
                        rpt.Logger = Me.Logger
                        LogVerbose(msg:=rtn & "::IS VALID = " & rpt.PrinterSettings.IsValid.ToString)
                        rpt.Print()
                        Try
                            rpt.Dispose()
                        Catch ex As Exception
                        Finally
                            rpt = Nothing
                        End Try
                    Case ePickingReportType.shippingDocument
                        Dim rpt As New SDI.Reports.shippingDoc
                        rpt.ExecutionPath = Me.ExecutionPath
                        LogVerbose(msg:=rtn & "::BUSINESS_UNIT = " & Me.BusinessUnit)
                        LogVerbose(msg:=rtn & "::BUSINESS_UNIT (INV) = " & Me.BusinessUnitINV)
                        rpt.SourceBusinessUnit = Me.BusinessUnit
                        LogVerbose(msg:=rtn & "::ORDER_NO = " & Me.OrderNo)
                        rpt.OrderNo = Me.OrderNo
                        rpt.oraCNstring = Me.oraCNstring
                        LogVerbose(msg:=rtn & "::PRINTER NAME = " & sitePrnName)
                        If (Me.PrintCopies > 0) Then
                            rpt.PrinterSettings.Copies = Me.PrintCopies
                        End If
                        Dim paperSrc As String = ""
                        Try
                            paperSrc = rpt.DefaultPageSettings.PaperSource.SourceName
                        Catch ex As Exception
                        End Try
                        LogVerbose(msg:=rtn & "::PAPER SOURCE INDEX = " & paperSrc)
                        rpt.PrinterSettings.PrinterName = sitePrnName
                        ' check/override target printer - site/ + print job type
                        If cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_DOCUMENT_PRINT_TO) Then
                            LogVerbose(msg:=rtn & "::(override) PRINTER NAME = " & CStr(cfgSite.KeyValue(Me.RPT_SHIPPING_DOCUMENT_PRINT_TO)))
                            rpt.PrinterSettings.PrinterName = CStr(cfgSite.KeyValue(Me.RPT_SHIPPING_DOCUMENT_PRINT_TO))
                            ' check/override paper source Id, since printer was overridden
                            If cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_DOCUMENT_PAPERSOURCE) Then
                                LogVerbose(msg:=rtn & "::(override) PAPER SOURCE INDEX = " & cfgSite.KeyValue(Me.RPT_SHIPPING_DOCUMENT_PAPERSOURCE))
                                rpt.DefaultPageSettings.PaperSource = rpt.PrinterSettings.PaperSources(CInt(cfgSite.KeyValue(Me.RPT_SHIPPING_DOCUMENT_PAPERSOURCE)))
                            End If
                        End If
                        ' check/override # of copies - site + print job type
                        If cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_DOCUMENT_COPY) Then
                            If IsNumeric(cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_DOCUMENT_COPY)) Then
                                LogVerbose(msg:=rtn & "::(override) COPIES = " & cfgSite.KeyValue(Me.RPT_SHIPPING_DOCUMENT_COPY))
                                rpt.PrinterSettings.Copies = CInt(cfgSite.KeyValue(Me.RPT_SHIPPING_DOCUMENT_COPY))
                            End If
                        End If
                        ' logger
                        rpt.Logger = Me.Logger
                        LogVerbose(msg:=rtn & "::IS VALID = " & rpt.PrinterSettings.IsValid.ToString)
                        rpt.Print()
                        Try
                            rpt.Dispose()
                        Catch ex As Exception
                        Finally
                            rpt = Nothing
                        End Try
                    Case ePickingReportType.shippingDocRegPick
                        Dim rpt As SDI.Reports.shippingDocRegPick = Nothing
                        '// print to printer
                        Try
                            rpt = New SDI.Reports.shippingDocRegPick
                            AddHandler rpt.AssignedPrintedPickedOrderLines, AddressOf Me.AssignedPrintedPickedOrderLinesHandler
                            rpt.ExecutionPath = Me.ExecutionPath
                            LogVerbose(msg:=rtn & "::BUSINESS_UNIT = " & Me.BusinessUnit)
                            LogVerbose(msg:=rtn & "::BUSINESS_UNIT (INV) = " & Me.BusinessUnitINV)
                            rpt.SourceBusinessUnit = Me.BusinessUnit
                            LogVerbose(msg:=rtn & "::ORDER_NO = " & Me.OrderNo)
                            rpt.OrderNo = Me.OrderNo
                            rpt.oraCNstring = Me.oraCNstring
                            LogVerbose(msg:=rtn & "::PRINTER NAME = " & sitePrnName)
                            If (Me.PrintCopies > 0) Then
                                rpt.PrinterSettings.Copies = Me.PrintCopies
                            End If
                            Dim paperSrc As String = ""
                            Try
                                paperSrc = rpt.DefaultPageSettings.PaperSource.SourceName
                            Catch ex As Exception
                            End Try
                            LogVerbose(msg:=rtn & "::PAPER SOURCE INDEX = " & paperSrc)
                            rpt.PrinterSettings.PrinterName = sitePrnName
                            ' check/override target printer - site/ + print job type
                            If cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_DOCUMENT_REG_PICK_PRINT_TO) Then
                                LogVerbose(msg:=rtn & "::(override) PRINTER NAME = " & CStr(cfgSite.KeyValue(Me.RPT_SHIPPING_DOCUMENT_REG_PICK_PRINT_TO)))
                                rpt.PrinterSettings.PrinterName = CStr(cfgSite.KeyValue(Me.RPT_SHIPPING_DOCUMENT_REG_PICK_PRINT_TO))
                                ' check/override paper source Id, since printer was overridden
                                If cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_DOCUMENT_REG_PICK_PAPERSOURCE) Then
                                    LogVerbose(msg:=rtn & "::(override) PAPER SOURCE INDEX = " & cfgSite.KeyValue(Me.RPT_SHIPPING_DOCUMENT_REG_PICK_PAPERSOURCE))
                                    rpt.DefaultPageSettings.PaperSource = rpt.PrinterSettings.PaperSources(CInt(cfgSite.KeyValue(Me.RPT_SHIPPING_DOCUMENT_REG_PICK_PAPERSOURCE)))
                                End If
                            End If
                            ' check/override # of copies - site + print job type
                            If cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_DOCUMENT_REG_PICK_COPY) Then
                                If IsNumeric(cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_DOCUMENT_REG_PICK_COPY)) Then
                                    LogVerbose(msg:=rtn & "::(override) COPIES = " & cfgSite.KeyValue(Me.RPT_SHIPPING_DOCUMENT_REG_PICK_COPY))
                                    rpt.PrinterSettings.Copies = CInt(cfgSite.KeyValue(Me.RPT_SHIPPING_DOCUMENT_REG_PICK_COPY))
                                End If
                            End If
                            '' user override target printer / papersource
                            'Try
                            '    If cfgSite.KeyValue.ContainsKey(Me.USER_OVERRIDE_USER_ID) Then
                            '        Dim cfgUserId As String = CStr(cfgSite.KeyValue(Me.USER_OVERRIDE_USER_ID)).Trim
                            '        If (cfgUserId.Length > 0) And (cfgUserId.ToUpper.IndexOf(m_userId.Trim.ToUpper) > -1) Then
                            '            ' printer
                            '            If cfgSite.KeyValue.ContainsKey(Me.USER_OVERRIDE_PRINT_TO) Then
                            '                LogVerbose(msg:=rtn & "::(user level override) PRINTER NAME = " & CStr(cfgSite.KeyValue(Me.USER_OVERRIDE_PRINT_TO)))
                            '                rpt.PrinterSettings.PrinterName = CStr(cfgSite.KeyValue(Me.USER_OVERRIDE_PRINT_TO))
                            '            End If
                            '            ' papersource
                            '            If cfgSite.KeyValue.ContainsKey(Me.USER_OVERRIDE_SHIPPING_DOCUMENT_REG_PICK_PAPERSOURCE) Then
                            '                LogVerbose(msg:=rtn & "::(user level override) PAPER SOURCE INDEX = " & cfgSite.KeyValue(Me.USER_OVERRIDE_SHIPPING_DOCUMENT_REG_PICK_PAPERSOURCE))
                            '                rpt.DefaultPageSettings.PaperSource = rpt.PrinterSettings.PaperSources(CInt(cfgSite.KeyValue(Me.USER_OVERRIDE_SHIPPING_DOCUMENT_REG_PICK_PAPERSOURCE)))
                            '            End If
                            '        End If
                            '    End If
                            'Catch ex As Exception
                            'End Try
                            ' inventory B/U override target printer / papersource
                            Try
                                If cfgSite.KeyValue.ContainsKey(Me.BU_INVENTORY_OVERRIDE_ID) Then
                                    Dim cfgBUs As String = CStr(cfgSite.KeyValue(Me.BU_INVENTORY_OVERRIDE_ID)).Trim
                                    If ((cfgBUs.Length > 0) And (Me.BusinessUnitINV.Trim.Length > 0) And (cfgBUs.ToUpper.IndexOf(Me.BusinessUnitINV.Trim.ToUpper) > -1)) Then
                                        ' printer
                                        If cfgSite.KeyValue.ContainsKey(Me.BU_INVENTORY_OVERRIDE_PRINT_TO) Then
                                            LogVerbose(msg:=rtn & "::(inventory b/u level override) PRINTER NAME = " & CStr(cfgSite.KeyValue(Me.BU_INVENTORY_OVERRIDE_PRINT_TO)))
                                            rpt.PrinterSettings.PrinterName = CStr(cfgSite.KeyValue(Me.BU_INVENTORY_OVERRIDE_PRINT_TO))
                                        End If
                                        ' papersource
                                        If cfgSite.KeyValue.ContainsKey(Me.BU_INVENTORY_OVERRIDE_SHIPPING_DOCUMENT_REG_PICK_PAPERSOURCE) Then
                                            LogVerbose(msg:=rtn & "::(inventory b/u level override) PAPER SOURCE INDEX = " & cfgSite.KeyValue(Me.BU_INVENTORY_OVERRIDE_SHIPPING_DOCUMENT_REG_PICK_PAPERSOURCE))
                                            rpt.DefaultPageSettings.PaperSource = rpt.PrinterSettings.PaperSources(CInt(cfgSite.KeyValue(Me.BU_INVENTORY_OVERRIDE_SHIPPING_DOCUMENT_REG_PICK_PAPERSOURCE)))
                                        End If
                                    End If
                                End If
                            Catch ex As Exception
                            End Try
                            ' partially-picked order filter
                            If cfgSite.KeyValue.ContainsKey(Me.RPT_PARTIAL_PICK_EXCLUDE_STATE) Then
                                ' override with what was in config, even when blank
                                LogVerbose(msg:=rtn & "::(override) PARTIAL PICK FILTER = " & CStr(cfgSite.KeyValue(Me.RPT_PARTIAL_PICK_EXCLUDE_STATE)).Trim)
                                rpt.PartialPickExcludeState = CStr(cfgSite.KeyValue(Me.RPT_PARTIAL_PICK_EXCLUDE_STATE)).Trim
                            End If
                            ' logger
                            rpt.Logger = Me.Logger
                            LogVerbose(msg:=rtn & "::IS VALID = " & rpt.PrinterSettings.IsValid.ToString)
                            Try
                                rpt.Print()
                            Catch ex As Exception
                                LogError(msg:=rtn & "::" & ex.ToString())
                            End Try
                            RemoveHandler rpt.AssignedPrintedPickedOrderLines, AddressOf Me.AssignedPrintedPickedOrderLinesHandler
                        Catch ex As Exception
                        End Try
                        Try
                            rpt.Dispose()
                        Catch ex As Exception
                        Finally
                            rpt = Nothing
                        End Try
                        ' DISABLE THIS!
                        '   this was the work-around when NYC's network is NOT working and for Wenjia to send these reports as fax - erwin
                        ''// print to file and rename
                        'Try
                        '    rpt = New SDI.Reports.shippingDocRegPick
                        '    rpt.ExecutionPath = Me.ExecutionPath
                        '    rpt.SourceBusinessUnit = Me.BusinessUnit
                        '    rpt.OrderNo = Me.OrderNo
                        '    rpt.oraCNstring = Me.oraCNstring
                        '    Try
                        '        rpt.PrinterSettings.PrinterName = "shipDoc"
                        '        rpt.print()
                        '    Catch ex As Exception
                        '        ' ignore
                        '    End Try
                        '    Try
                        '        rpt.Dispose()
                        '    Catch ex As Exception
                        '    Finally
                        '        rpt = Nothing
                        '    End Try
                        '    Dim ctr As Integer = 0
                        '    Dim s As String = "MMddHHmmssffff"
                        '    While True
                        '        Threading.Thread.Sleep(3000)
                        '        Dim fi As New System.IO.FileInfo(fileName:="c:\Temp\nyc.printouts\shippingDocs\shippingDoc.output")
                        '        If Not (fi Is Nothing) Then
                        '            If fi.Exists Then
                        '                fi.MoveTo(destFileName:="c:\Temp\nyc.printouts\shippingDocs\sd" & Now.ToString(Format:=s) & ".output")
                        '            Else
                        '                Exit While
                        '            End If
                        '        End If
                        '        fi = Nothing
                        '        ctr += 1
                        '        If ctr > 5 Then
                        '            Exit While
                        '        End If
                        '    End While
                        'Catch ex As Exception
                        'End Try
                    Case ePickingReportType.packingSlipRegPick
                        Dim rpt As SDI.Reports.shippingLabelRegPick = Nothing
                        '// print to printer
                        Try
                            rpt = New SDI.Reports.shippingLabelRegPick
                            rpt.ExecutionPath = Me.ExecutionPath
                            LogVerbose(msg:=rtn & "::BUSINESS_UNIT = " & Me.BusinessUnit)
                            LogVerbose(msg:=rtn & "::BUSINESS_UNIT (INV) = " & Me.BusinessUnitINV)
                            rpt.SourceBusinessUnit = Me.BusinessUnit
                            LogVerbose(msg:=rtn & "::ORDER_NO = " & Me.OrderNo)
                            rpt.OrderNo = Me.OrderNo
                            LogVerbose(msg:=rtn & "::CN string = " & Me.oraCNstring)
                            rpt.oraCNstring = Me.oraCNstring
                            LogVerbose(msg:=rtn & "::PRINTER NAME = " & sitePrnName)
                            If (Me.PrintCopies > 0) Then
                                rpt.PrinterSettings.Copies = Me.PrintCopies
                            End If
                            Dim paperSrc As String = ""
                            Try
                                paperSrc = rpt.DefaultPageSettings.PaperSource.SourceName
                            Catch ex As Exception
                            End Try
                            LogVerbose(msg:=rtn & "::PAPER SOURCE INDEX = " & paperSrc)
                            rpt.PrinterSettings.PrinterName = sitePrnName
                            ' check/override with this printer name
                            If cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_LABEL_REG_PICK_PRINT_TO) Then
                                LogVerbose(msg:=rtn & "::(override) PRINTER NAME = " & CStr(cfgSite.KeyValue(Me.RPT_SHIPPING_LABEL_REG_PICK_PRINT_TO)))
                                rpt.PrinterSettings.PrinterName = CStr(cfgSite.KeyValue(Me.RPT_SHIPPING_LABEL_REG_PICK_PRINT_TO))
                                ' check/override paper source Id, since printer was overridden
                                If cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_LABEL_REG_PICK_PAPERSOURCE) Then
                                    LogVerbose(msg:=rtn & "::(override) PAPER SOURCE INDEX = " & cfgSite.KeyValue(Me.RPT_SHIPPING_LABEL_REG_PICK_PAPERSOURCE))
                                    rpt.DefaultPageSettings.PaperSource = rpt.PrinterSettings.PaperSources(CInt(cfgSite.KeyValue(Me.RPT_SHIPPING_LABEL_REG_PICK_PAPERSOURCE)))
                                End If
                            End If
                            ' check/override # of copies - site + print job type
                            If cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_LABEL_REG_PICK_COPY) Then
                                If IsNumeric(cfgSite.KeyValue.ContainsKey(Me.RPT_SHIPPING_LABEL_REG_PICK_COPY)) Then
                                    LogVerbose(msg:=rtn & "::(override) COPIES = " & cfgSite.KeyValue(Me.RPT_SHIPPING_LABEL_REG_PICK_COPY))
                                    rpt.PrinterSettings.Copies = CInt(cfgSite.KeyValue(Me.RPT_SHIPPING_LABEL_REG_PICK_COPY))
                                End If
                            End If
                            '' user override target printer
                            'Try
                            '    If cfgSite.KeyValue.ContainsKey(Me.USER_OVERRIDE_USER_ID) Then
                            '        Dim cfgUserId As String = CStr(cfgSite.KeyValue(Me.USER_OVERRIDE_USER_ID)).Trim
                            '        If (cfgUserId.Length > 0) And (cfgUserId.ToUpper.IndexOf(m_userId.Trim.ToUpper) > -1) Then
                            '            ' printer
                            '            If cfgSite.KeyValue.ContainsKey(Me.USER_OVERRIDE_PRINT_TO) Then
                            '                LogVerbose(msg:=rtn & "::(user level override) PRINTER NAME = " & CStr(cfgSite.KeyValue(Me.USER_OVERRIDE_PRINT_TO)))
                            '                rpt.PrinterSettings.PrinterName = CStr(cfgSite.KeyValue(Me.USER_OVERRIDE_PRINT_TO))
                            '            End If
                            '            ' papersource
                            '            If cfgSite.KeyValue.ContainsKey(Me.USER_OVERRIDE_SHIPPING_LABEL_REG_PICK_PAPERSOURCE) Then
                            '                LogVerbose(msg:=rtn & "::(user level override) PAPER SOURCE INDEX = " & cfgSite.KeyValue(Me.USER_OVERRIDE_SHIPPING_LABEL_REG_PICK_PAPERSOURCE))
                            '                rpt.DefaultPageSettings.PaperSource = rpt.PrinterSettings.PaperSources(CInt(cfgSite.KeyValue(Me.USER_OVERRIDE_SHIPPING_LABEL_REG_PICK_PAPERSOURCE)))
                            '            End If
                            '        End If
                            '    End If
                            'Catch ex As Exception
                            'End Try
                            ' inventory B/U override target printer / papersource
                            Try
                                If cfgSite.KeyValue.ContainsKey(Me.BU_INVENTORY_OVERRIDE_ID) Then
                                    Dim cfgBUs As String = CStr(cfgSite.KeyValue(Me.BU_INVENTORY_OVERRIDE_ID)).Trim
                                    If ((cfgBUs.Length > 0) And (Me.BusinessUnitINV.Trim.Length > 0) And (cfgBUs.ToUpper.IndexOf(Me.BusinessUnitINV.Trim.ToUpper) > -1)) Then
                                        ' printer
                                        If cfgSite.KeyValue.ContainsKey(Me.BU_INVENTORY_OVERRIDE_PRINT_TO) Then
                                            LogVerbose(msg:=rtn & "::(inventory b/u level override) PRINTER NAME = " & CStr(cfgSite.KeyValue(Me.BU_INVENTORY_OVERRIDE_PRINT_TO)))
                                            rpt.PrinterSettings.PrinterName = CStr(cfgSite.KeyValue(Me.BU_INVENTORY_OVERRIDE_PRINT_TO))
                                        End If
                                        ' papersource
                                        If cfgSite.KeyValue.ContainsKey(Me.BU_INVENTORY_OVERRIDE_SHIPPING_LABEL_REG_PICK_PAPERSOURCE) Then
                                            LogVerbose(msg:=rtn & "::(inventory b/u level override) PAPER SOURCE INDEX = " & cfgSite.KeyValue(Me.BU_INVENTORY_OVERRIDE_SHIPPING_LABEL_REG_PICK_PAPERSOURCE))
                                            rpt.DefaultPageSettings.PaperSource = rpt.PrinterSettings.PaperSources(CInt(cfgSite.KeyValue(Me.BU_INVENTORY_OVERRIDE_SHIPPING_LABEL_REG_PICK_PAPERSOURCE)))
                                        End If
                                    End If
                                End If
                            Catch ex As Exception
                            End Try
                            ' partially-picked order filter
                            If cfgSite.KeyValue.ContainsKey(Me.RPT_PARTIAL_PICK_EXCLUDE_STATE) Then
                                ' override with what was in config, even when blank
                                LogVerbose(msg:=rtn & "::(override) PARTIAL PICK FILTER = " & CStr(cfgSite.KeyValue(Me.RPT_PARTIAL_PICK_EXCLUDE_STATE)).Trim)
                                rpt.PartialPickExcludeState = CStr(cfgSite.KeyValue(Me.RPT_PARTIAL_PICK_EXCLUDE_STATE)).Trim
                            End If
                            ' logger
                            rpt.Logger = Me.Logger
                            LogVerbose(msg:=rtn & "::IS VALID = " & rpt.PrinterSettings.IsValid.ToString)
                            Try
                                rpt.Print()
                            Catch ex As Exception
                                LogError(msg:=rtn & "::" & ex.ToString())
                            End Try
                        Catch ex As Exception
                        End Try
                        Try
                            rpt.Dispose()
                        Catch ex As Exception
                        Finally
                            rpt = Nothing
                        End Try
                        ' DISABLE THIS!
                        '   this was the work-around when NYC's network is NOT working and for Wenjia to send these reports as fax - erwin
                        ''// print to file and rename
                        'Try
                        '    rpt = New SDI.Reports.shippingLabelRegPick
                        '    rpt.ExecutionPath = Me.ExecutionPath
                        '    rpt.SourceBusinessUnit = Me.BusinessUnit
                        '    rpt.OrderNo = Me.OrderNo
                        '    rpt.oraCNstring = Me.oraCNstring
                        '    Try
                        '        rpt.PrinterSettings.PrinterName = "packSlip"
                        '        rpt.Print()
                        '    Catch ex As Exception
                        '        ' ignore
                        '    End Try
                        '    Try
                        '        rpt.Dispose()
                        '    Catch ex As Exception
                        '    Finally
                        '        rpt = Nothing
                        '    End Try
                        '    Dim s As String = "MMddHHmmssffff"
                        '    Dim ctr As Integer = 0
                        '    While True
                        '        Threading.Thread.Sleep(3000)
                        '        Dim fi As New System.IO.FileInfo(fileName:="c:\Temp\nyc.printouts\packingSlips\packingSlip.output")
                        '        If Not (fi Is Nothing) Then
                        '            If fi.Exists Then
                        '                fi.MoveTo(destFileName:="c:\Temp\nyc.printouts\packingSlips\ps" & Now.ToString(Format:=s) & ".output")
                        '            Else
                        '                Exit While
                        '            End If
                        '        End If
                        '        fi = Nothing
                        '        ctr += 1
                        '        If ctr > 5 Then
                        '            Exit While
                        '        End If
                        '    End While
                        'Catch ex As Exception
                        'End Try
                End Select
            Next
        End If

    End Sub

    Private Function GetPrinterNameForSite(ByVal bu As String, _
                                           ByVal oraCN As OleDb.OleDbConnection) As String
        Dim rtn As String = "ReportGenerator.GetPrinterNameForSite"
        Dim prn As String = ""

        Try
            bu = "I0" & bu.Trim.ToUpper.Substring(2, bu.Trim.Length - 2).PadLeft(3, CChar("0"))
        Catch ex As Exception
        End Try

        Dim bCNOpenedHere As Boolean = Not (oraCN.State = ConnectionState.Open)

        If Not (oraCN.State = ConnectionState.Open) Then
            oraCN.Open()
        End If

        Dim sql As String = "" & _
                            "SELECT A.ISA_SITE_PRINTER " & vbCrLf & _
                            "FROM PS_ISA_ENTERPRISE A " & vbCrLf & _
                            "WHERE A.ISA_BUSINESS_UNIT = '" & bu & "' " & vbCrLf & _
                            ""

        Dim cmd As OleDb.OleDbCommand = oraCN.CreateCommand

        cmd.CommandText = sql
        cmd.CommandType = CommandType.Text

        Dim rdr As OleDb.OleDbDataReader = cmd.ExecuteReader

        If Not (rdr Is Nothing) Then
            ' get the first record ... should only return One
            If rdr.Read Then
                prn = CStr(rdr("ISA_SITE_PRINTER"))
            End If
        End If

        Try
            rdr.Close()
        Catch ex As Exception
        Finally
            rdr = Nothing
        End Try

        Try
            cmd.Dispose()
        Catch ex As Exception
        Finally
            cmd = Nothing
        End Try

        If bCNOpenedHere Then
            Try
                oraCN.Close()
            Catch ex As Exception
            End Try
        End If

        Return prn
    End Function

    Private Function CheckLoadOverrides(ByVal bu3 As String) As siteConfig
        Dim cfgSite As siteConfig = New siteConfig(siteId:=bu3)
        Try
            bu3 = bu3.Trim.ToUpper

            Dim configDir As String = System.Reflection.Assembly.GetExecutingAssembly.GetModules()(0).FullyQualifiedName

            If configDir.Length > 0 Then
                configDir = configDir.Substring(startIndex:=0, length:=configDir.Length - (configDir.Length - configDir.LastIndexOf(CType("\", Char))))
            Else
                configDir = ""
            End If

            configDir &= "\"

            ' override (if provided)
            If Me.ExecutionPath.Trim.Length > 0 Then
                configDir = Me.ExecutionPath
            End If

            Dim ass As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly
            Dim assName As System.Reflection.AssemblyName = ass.GetName
            Dim dllName As String = assName.Name
            Dim xmlRdr As System.Xml.XmlTextReader = New System.Xml.XmlTextReader(configDir & dllName & ".dll.config")

            ' read the file
            xmlRdr.WhitespaceHandling = Xml.WhitespaceHandling.None

            While (xmlRdr.Read)
                Select Case xmlRdr.NodeType
                    Case Xml.XmlNodeType.Element
                        Dim strKey As String = ""
                        Dim strVal As String = ""
                        If xmlRdr.HasAttributes And xmlRdr.Name.Trim.ToUpper = "ADD" Then
                            strKey = xmlRdr.GetAttribute("bu")
                            strVal = xmlRdr.GetAttribute("value")
                            If strKey.Trim.ToUpper = bu3 Then
                                cfgSite = ParseSiteConfigOverride(bu3, strVal)
                            End If
                        End If
                End Select
            End While

            xmlRdr.Close()
            xmlRdr = Nothing

            ' just cleaning it up here... good thing it's private =D
            '   - I'm wondering why these objects does not have a Dispose method??? not disposable!
            '   - ass.EntryPoint (what do you think this is?)
            '   - ass.FullName (this is NOT good!!! I guess it's public within that object!?)
            '   - ass.Evidence (why would anybody would ask for this? what does this return?)
            '   - ass.Location (at least this one could be helpfull ;D)
            '   - assName.Clone (why would anyone want to do this???)
            '   - assName.CultureInfo (I don't think I would care about this!? would you?)
            '   - this type of object can also be put into an array ... a collection of ...
            '   - AVOID declaring this as PUBLIC
            '   - can be a FRIEND, PROTECTED or SHARED too (imagine if it is declared PUBLIC SHARED ...)
            '   - these are the methods and properties on the .NET Framework v1.1,
            '     what do you think will be the improvement on the upcoming versions?
            assName = Nothing
            ass = Nothing

        Catch ex As Exception
            ' ...
        End Try

        Return cfgSite
    End Function

    '// expecting key/value pairs ... something like ... for a specific business unit
    '//     230 - "printToOverride=\\sdi\mudshark;SDI.Reports.shippingDoc.paperSourceId=0;SDI.Reports.shippingDoc.copy=1;SDI.Reports.shippingLabel.paperSourceId=0;SDI.Reports.shippingLabel.copy=1"
    Private Function ParseSiteConfigOverride(ByVal buId As String, ByVal s As String) As siteConfig
        Dim site As New siteConfig(siteId:=buId)
        s = s.Trim
        Dim sections As String() = s.Split(CChar(";"))
        For Each keyVal As String In sections
            If keyVal.Trim.Length > 0 Then
                Dim s1 As String() = keyVal.Trim.Split(CChar("="))
                ' key
                Dim id As String = ""
                Try
                    id = s1(0).Trim
                Catch ex As Exception
                End Try
                ' value
                Dim val As String = ""
                Try
                    val = s1(1).Trim
                Catch ex As Exception
                End Try
                ' check/add
                If id.Length > 0 Then
                    If site.KeyValue.ContainsKey(id) Then
                        ' OVERRIDE
                        site.KeyValue(id) = val
                    Else
                        ' ADD
                        site.KeyValue.Add(id, val)
                    End If
                End If
            End If
        Next
        Return site
    End Function

    Private Sub ListAvailPrintersAndPaperSources(ByVal cfgSite As siteConfig)
        Try
            If Not (cfgSite Is Nothing) Then
                ' for listing installed printers on this machine
                Dim bIsListAvailPrinters As Boolean = False
                Try
                    If cfgSite.KeyValue.ContainsKey(Me.DEBUG_LIST_AVAILABLE_PRINTERS) Then
                        bIsListAvailPrinters = CBool(cfgSite.KeyValue(Me.DEBUG_LIST_AVAILABLE_PRINTERS))
                    End If
                Catch ex As Exception
                End Try

                ' for listing available paper sources for printer's available on this machine
                Dim bIsListAvailPrnPaperSources As Boolean = False
                Try
                    ' only consider if we're going to list available printers
                    If bIsListAvailPrinters Then
                        If cfgSite.KeyValue.ContainsKey(Me.DEBUG_LIST_AVAIL_PRN_PAPERSOURCES) Then
                            bIsListAvailPrnPaperSources = CBool(cfgSite.KeyValue(Me.DEBUG_LIST_AVAIL_PRN_PAPERSOURCES))
                        End If
                    End If
                Catch ex As Exception
                End Try

                ' for debugging purposes
                If bIsListAvailPrinters Then
                    If System.Drawing.Printing.PrinterSettings.InstalledPrinters.Count > 0 Then
                        Dim s As String = ""
                        Dim prn As System.Drawing.Printing.PrinterSettings = Nothing
                        For i As Integer = 0 To (System.Drawing.Printing.PrinterSettings.InstalledPrinters.Count - 1)
                            prn = New System.Drawing.Printing.PrinterSettings
                            prn.PrinterName = CStr(System.Drawing.Printing.PrinterSettings.InstalledPrinters(i))
                            s = ""
                            ' printer
                            s &= "" & _
                                 "[ Printer ]" & vbCrLf & _
                                 "  Printer Name    : " & prn.PrinterName & vbCrLf & _
                                 "  Is Valid Printer: " & prn.IsValid.ToString & vbCrLf & _
                                 ""
                            s &= vbCrLf
                            ' default page settings
                            s &= "" & _
                                 "[ Default Page Settings ]" & vbCrLf & _
                                 "  Paper Source: " & prn.DefaultPageSettings.PaperSource.SourceName & vbCrLf & _
                                 "  Paper Name  : " & prn.DefaultPageSettings.PaperSize.PaperName & vbCrLf & _
                                 ""
                            s &= vbCrLf
                            ' available paper sources
                            If bIsListAvailPrnPaperSources Then
                                s &= "" & _
                                     "[ Available Paper Source ]" & vbCrLf & _
                                     ""
                                If prn.PaperSources.Count > 0 Then
                                    For nIdx As Integer = 0 To (prn.PaperSources.Count - 1)
                                        s &= "  " & nIdx.ToString & vbTab & prn.PaperSources(nIdx).SourceName.Trim & vbCrLf
                                    Next
                                Else
                                    s &= "  NO PAPER SOURCES" & vbCrLf
                                End If
                            End If
                            LogVerbose(msg:=s)
                        Next
                        prn = Nothing
                    End If
                End If
            End If
        Catch ex As Exception
            ' just ignore
            '   we don't care
        End Try
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

    Private Sub AssignedPrintedPickedOrderLinesHandler(ByVal sender As Object, ByVal e As EventArgs)
        ' copy underlying report's order line lists
        Me.PrintedPickedOrderLines.Items.Clear()
        If (TypeOf sender Is SDI.Reports.ISDiReport) Then
            Dim arr As SDI.Reports.PrintedPickedOrderLines = CType(sender, SDI.Reports.ISDiReport).PrintedPickedOrderLines
            If Not (arr Is Nothing) Then
                For Each item As SDI.Reports.PrintedPickedOrderLine In arr.Items
                    Me.PrintedPickedOrderLines.Items.Add(item)
                Next
            End If
        End If
        ' then, we'll just have to echo this event out ...
        RaiseEvent AssignedPrintedPickedOrderLines(Me, Nothing)
    End Sub

    Private m_lines As SDI.Reports.PrintedPickedOrderLines = Nothing

    Public ReadOnly Property PrintedPickedOrderLines() As SDI.Reports.PrintedPickedOrderLines Implements SDI.Reports.ISDiReport.PrintedPickedOrderLines
        Get
            If (m_lines Is Nothing) Then
                m_lines = New SDI.Reports.PrintedPickedOrderLines
            End If
            Return m_lines
        End Get
    End Property

End Class
