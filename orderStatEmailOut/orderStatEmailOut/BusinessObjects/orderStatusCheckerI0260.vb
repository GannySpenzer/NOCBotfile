Imports System.Data
Imports System.Data.OleDb


Public Class orderStatusCheckerI0260

    Implements ISiteSetting
    Implements ICanCheckOrderShippedStatus
    Implements ICanCheckNSTKOrderShippedStatus

    Public Sub New()

    End Sub

    Public Sub New(ByVal siteId As String)
        m_siteId = siteId
    End Sub

    Private Const APP_SETTING_SCANNED_DOC_LINK As String = "scannedDocumentLink"
    Private Const APP_SETTING_DEVICE_DELIV_DOC_LINK As String = "urlTruckShipmentBOL"
    Private Const APP_SETTING_LEGATO_CN As String = "legatoSQLCNstring"

    Private m_oraCN As OleDbConnection = Nothing
    Private m_logger As SDI.ApplicationLogger.IApplicationLogger = Nothing
    Private m_appSettings As Hashtable = Nothing
    Private m_scannedDocLinkURL As String = "http://legato.sdi.com/wx/query.asp?ORDERNUM={0}"
    Private m_deviceDeliveredDocLinkURL As String = "http://www.insiteonline.com/InSiteOnlineTrucking/shipmentBOL.aspx?bu={0}&ord={1}"
    Private m_legatoCNstring As String = "server=ANGEL;uid=NYCSHIP;pwd=nycship;initial catalog='OTG';"


#Region " ISiteSetting Implementations "

    Private m_siteId As String = ""

    Public Property SiteId() As String Implements ISiteSetting.SiteId
        Get
            Return m_siteId
        End Get
        Set(ByVal Value As String)
            m_siteId = Value
        End Set
    End Property

    Private m_siteName As String = ""

    Public Property SiteName() As String Implements ISiteSetting.SiteName
        Get
            Return m_siteName
        End Get
        Set(ByVal Value As String)
            m_siteName = Value
        End Set
    End Property

    ' default to date part of today
    Private m_startDT As DateTime = Now.Date

    Public Property StartDate() As Date Implements ISiteSetting.StartDate
        Get
            Return m_startDT
        End Get
        Set(ByVal Value As DateTime)
            m_startDT = Value
        End Set
    End Property

    '// array of statusToCheck object type
    Private m_arrStatusesToCheck As New ArrayList

    Public ReadOnly Property StatusesToCheck() As System.Collections.ArrayList Implements ISiteSetting.StatusesToCheck
        Get
            Return m_arrStatusesToCheck
        End Get
    End Property

    Private m_bccForAll As String = ""

    Public Property [bccForAll]() As String Implements ISiteSetting.bccForAll
        Get
            Return m_bccForAll
        End Get
        Set(ByVal Value As String)
            m_bccForAll = Value
        End Set
    End Property

    Private m_bccForNSTK As String = ""

    Public Property [bccForNSTK]() As String Implements ISiteSetting.bccForNSTK
        Get
            Return m_bccForNSTK
        End Get
        Set(ByVal Value As String)
            m_bccForNSTK = Value
        End Set
    End Property

    Private m_bccForNY0A As String = ""

    Public Property [bccForNY0A]() As String Implements ISiteSetting.bccForNY0A
        Get
            Return m_bccForNY0A
        End Get
        Set(ByVal Value As String)
            m_bccForNY0A = Value
        End Set
    End Property

#End Region

#Region " ICanCheckOrderShippedStatus Implementations "

    Public Sub checkOrderShippedStatus(ByVal sender As Object, ByVal e As orderShippedStatusEventArgs) Implements ICanCheckOrderShippedStatus.checkOrderShippedStatus

    End Sub

    Public Sub sendOrderShippedStatusEmail(ByVal sender As Object, ByVal e As orderShippedStatusEventArgs) Implements ICanCheckOrderShippedStatus.sendOrderShippedStatusEmail
        m_oraCN = e.oledbCN
        m_logger = e.Logger
        m_appSettings = e.AppSettings

        ' get link for scanned document
        Dim s As String = ""
        Try
            s = CStr(m_appSettings(Me.APP_SETTING_SCANNED_DOC_LINK))
            If (s Is Nothing) Then
                s = ""
            End If
        Catch ex As Exception
        End Try
        If s.Length > 0 Then
            m_scannedDocLinkURL = s
        End If
        ' get link for device delivered orders
        Try
            s = CStr(m_appSettings(Me.APP_SETTING_DEVICE_DELIV_DOC_LINK))
            If (s Is Nothing) Then
                s = ""
            End If
        Catch ex As Exception
        End Try
        If s.Length > 0 Then
            m_deviceDeliveredDocLinkURL = s
        End If
        ' get legato connection string (SQL Server)
        Try
            s = CStr(m_appSettings(Me.APP_SETTING_LEGATO_CN))
            If (s Is Nothing) Then
                s = ""
            End If
        Catch ex As Exception
        End Try
        If s.Length > 0 Then
            m_legatoCNstring = s
        End If

        Const flag_FAB_ORDER As String = "FAB"

        Dim dtStart As DateTime = Me.StartDate
        Dim dtEnd As DateTime = Now
        Dim arrOrders As ArrayList = Me.getShippedOrders(Me.SiteId, _
                                                         dtStart, _
                                                         dtEnd)
        m_logger.WriteVerboseLog("checking " & arrOrders.Count.ToString & " order(s)/order line(s) ...")
        If arrOrders.Count > 0 Then
            For Each ord As changedOrder In arrOrders

                Dim arr As New ArrayList
                Dim myOrder As orderHdr = Nothing
                Dim ordLine As orderLine = Nothing

                If m_oraCN.State <> ConnectionState.Closed Then
                    m_oraCN.Close()
                End If
                m_oraCN.Open()

                '"           LOC.DESCR || '<br>' || " & vbCrLf & _
                '"           LOC.ADDRESS1 || '<br>' || " & vbCrLf & _
                '"           LOC.ADDRESS2 || '<br>' || " & vbCrLf & _
                '"           LOC.ADDRESS3 || '<br>' || " & vbCrLf & _
                '"           LOC.CITY || ', ' || LOC.STATE || ' ' || LOC.POSTAL " & vbCrLf & _

                '// grab order data
                '      Dim sql As String = "" & _
                '"SELECT " & vbCrLf & _
                '" A.BUSINESS_UNIT_OM AS BUSINESS_UNIT " & vbCrLf & _
                '",A.ORDER_NO AS ORDER_NO " & vbCrLf & _
                '",D.INTFC_LINE_NUM AS LINE_NBR " & vbCrLf & _
                '",E.ORDER_INT_LINE_NO AS ORDER_INT_LINE_NO " & vbCrLf & _
                '",E.DEMAND_LINE_NO AS DEMAND_LINE_NO " & vbCrLf & _
                '",B.EMPLID AS ISA_EMPLOYEE_ID " & vbCrLf & _
                '",(C.FIRST_NAME_SRCH || ' ' || C.LAST_NAME_SRCH) AS ISA_EMPLOYEE_NAME " & vbCrLf & _
                '",C.ISA_EMPLOYEE_EMAIL AS ISA_EMPLOYEE_EMAIL " & vbCrLf & _
                '",I.INV_ITEM_ID AS ITEM_ID " & vbCrLf & _
                '",I.DESCR60 AS ITEM_DESC " & vbCrLf & _
                '",E.QTY_PICKED AS QTY_PICKED " & vbCrLf & _
                '",E.QTY_SHIPPED AS QTY_SHIPPED " & vbCrLf & _
                '",E.SHIP_DATE AS SHIP_DATE " & vbCrLf & _
                '",F.SHIPTO_ID AS SHIPTO_ID " & vbCrLf & _
                '",B.ISA_MACHINE_NO AS ISA_MACHINE_NO " & vbCrLf & _
                '",B.ISA_WORK_ORDER_NO AS ISA_WORK_ORDER_NO " & vbCrLf & _
                '" ,(" & vbCrLf & _
                '"   SELECT LOC.DESCR AS DESCR " & vbCrLf & _
                '"   FROM PS_LOCATION_TBL LOC" & vbCrLf & _
                '"   WHERE LOC.SETID='MAIN1'" & vbCrLf & _
                '"     AND LOC.EFF_STATUS = 'A'" & vbCrLf & _
                '"     AND LOC.LOCATION = F.SHIPTO_ID" & vbCrLf & _
                '"     AND LOC.EFFDT = (" & vbCrLf & _
                '"                      SELECT MAX(LOC1.EFFDT)" & vbCrLf & _
                '"                      FROM PS_LOCATION_TBL LOC1" & vbCrLf & _
                '"                      WHERE LOC1.SETID = LOC.SETID" & vbCrLf & _
                '"                        AND LOC1.LOCATION = LOC.LOCATION" & vbCrLf & _
                '"                        AND LOC1.EFF_STATUS = LOC.EFF_STATUS" & vbCrLf & _
                '"                        AND LOC1.EFFDT <= SYSDATE" & vbCrLf & _
                '"                     )" & vbCrLf & _
                '"     AND ROWNUM < 2" & vbCrLf & _
                '"  ) AS SHIPTO_LOC" & vbCrLf & _
                '"FROM " & vbCrLf & _
                '" PS_ISA_ORD_INTFC_H A" & vbCrLf & _
                '",PS_ISA_ORD_INTFC_L B" & vbCrLf & _
                '",(" & vbCrLf & _
                '"  SELECT " & vbCrLf & _
                '"   USR.ISA_USER_ID " & vbCrLf & _
                '"  ,USR.FIRST_NAME_SRCH " & vbCrLf & _
                '"  ,USR.LAST_NAME_SRCH " & vbCrLf & _
                '"  ,USR.BUSINESS_UNIT " & vbCrLf & _
                '"  ,USR.ISA_EMPLOYEE_ID " & vbCrLf & _
                '"  ,USR.ISA_EMPLOYEE_NAME " & vbCrLf & _
                '"  ,USR.ISA_EMPLOYEE_EMAIL " & vbCrLf & _
                '"  FROM PS_ISA_USERS_TBL USR " & vbCrLf & _
                '"  WHERE USR.BUSINESS_UNIT = '" & ord.BusinessUnit & "' " & vbCrLf & _
                '" ) C " & vbCrLf & _
                '",PS_ISA_ORD_INTFC_O D" & vbCrLf & _
                '",PS_MASTER_ITEM_TBL I" & vbCrLf & _
                '",PS_SHIP_INF_INV E" & vbCrLf & _
                '",(" & vbCrLf & _
                '"  SELECT LD.ORDER_NO, LD.SHIPTO_ID, LD.LOAD_ID " & vbCrLf & _
                '"  FROM PS_ISA_LOAD_INT LD " & vbCrLf & _
                '"  WHERE LD.ORDER_NO = '" & ord.OrderNo & "' " & vbCrLf & _
                '"    AND ROWNUM < 2 " & vbCrLf & _
                '" ) F " & vbCrLf & _
                '"WHERE A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
                '"  AND A.BUSINESS_UNIT_OM = '" & ord.BusinessUnit & "' " & vbCrLf & _
                '"  AND A.ORDER_NO = '" & ord.OrderNo & "' " & vbCrLf & _
                '"  AND '" & ord.BusinessUnit & "' = C.BUSINESS_UNIT (+) " & vbCrLf & _
                '"  AND B.EMPLID = C.ISA_EMPLOYEE_ID (+) " & vbCrLf & _
                '"  AND A.BUSINESS_UNIT_OM = D.BUSINESS_UNIT_OM " & vbCrLf & _
                '"  AND A.ORDER_NO = D.ORDER_NO " & vbCrLf & _
                '"  AND B.LINE_NBR = D.INTFC_LINE_NUM " & vbCrLf & _
                '"  AND 'OM' = E.DEMAND_SOURCE " & vbCrLf & _
                '"  AND A.BUSINESS_UNIT_OM = E.SOURCE_BUS_UNIT " & vbCrLf & _
                '"  AND D.ORDER_NO = E.ORDER_NO " & vbCrLf & _
                '"  AND D.ORDER_INT_LINE_NO = E.ORDER_INT_LINE_NO " & vbCrLf & _
                '"  AND 'MAIN1' = I.SETID " & vbCrLf & _
                '"  AND E.INV_ITEM_ID = I.INV_ITEM_ID " & vbCrLf & _
                '"  AND E.SHIP_DATE IS NOT NULL " & vbCrLf & _
                '"  AND A.ORDER_NO = F.ORDER_NO " & vbCrLf & _
                '"  AND B.INV_ITEM_ID <> ' ' " & vbCrLf & _
                '"  AND NOT EXISTS (" & vbCrLf & _
                '"                  SELECT 'X'" & vbCrLf & _
                '"                  FROM PS_ISA_ORDSTAT_EML G" & vbCrLf & _
                '"                  WHERE A.BUSINESS_UNIT_OM = G.BUSINESS_UNIT_OM" & vbCrLf & _
                '"                    AND A.ORDER_NO = G.ORDER_NO" & vbCrLf & _
                '"                    AND D.INTFC_LINE_NUM = G.LINE_NBR" & vbCrLf & _
                '"                    AND E.ORDER_INT_LINE_NO = G.ORDER_INT_LINE_NO" & vbCrLf & _
                '"                    AND E.DEMAND_LINE_NO = G.DEMAND_LINE_NO" & vbCrLf & _
                '"                    AND G.ISA_ORDER_STATUS = '7'" & vbCrLf & _
                '"                 )" & vbCrLf & _
                '"ORDER BY A.BUSINESS_UNIT_OM, A.ORDER_NO, B.EMPLID, D.INTFC_LINE_NUM " & vbCrLf & _
                '             ""
                ' added a UNION to pick-up non-stock order/order items
                Dim sql As String = "" & _
                                    "SELECT " & vbCrLf & _
                                    " X.BUSINESS_UNIT " & vbCrLf & _
                                    ",X.ORDER_NO " & vbCrLf & _
                                    ",X.LINE_NBR " & vbCrLf & _
                                    ",X.ORDER_INT_LINE_NO " & vbCrLf & _
                                    ",X.DEMAND_LINE_NO " & vbCrLf & _
                                    ",X.ISA_EMPLOYEE_ID " & vbCrLf & _
                                    ",X.ISA_EMPLOYEE_NAME " & vbCrLf & _
                                    ",X.ISA_EMPLOYEE_EMAIL " & vbCrLf & _
                                    ",X.ITEM_ID " & vbCrLf & _
                                    ",X.ITEM_DESC " & vbCrLf & _
                                    ",X.QTY_PICKED " & vbCrLf & _
                                    ",X.QTY_SHIPPED " & vbCrLf & _
                                    ",X.SHIP_DATE " & vbCrLf & _
                                    ",X.SHIPTO_ID " & vbCrLf & _
                                    ",X.ISA_MACHINE_NO " & vbCrLf & _
                                    ",X.ISA_WORK_ORDER_NO " & vbCrLf & _
                                    ",X.SHIPTO_LOC " & vbCrLf & _
                                    "FROM " & vbCrLf & _
                                    "(" & vbCrLf & _
                                    " SELECT " & vbCrLf & _
                                    "  A.BUSINESS_UNIT_OM AS BUSINESS_UNIT " & vbCrLf & _
                                    " ,A.ORDER_NO AS ORDER_NO " & vbCrLf & _
                                    " ,D.INTFC_LINE_NUM AS LINE_NBR " & vbCrLf & _
                                    " ,E.ORDER_INT_LINE_NO AS ORDER_INT_LINE_NO " & vbCrLf & _
                                    " ,E.DEMAND_LINE_NO AS DEMAND_LINE_NO " & vbCrLf & _
                                    " ,B.EMPLID AS ISA_EMPLOYEE_ID " & vbCrLf & _
                                    " ,(C.FIRST_NAME_SRCH || ' ' || C.LAST_NAME_SRCH) AS ISA_EMPLOYEE_NAME " & vbCrLf & _
                                    " ,C.ISA_EMPLOYEE_EMAIL AS ISA_EMPLOYEE_EMAIL " & vbCrLf & _
                                    " ,I.INV_ITEM_ID AS ITEM_ID " & vbCrLf & _
                                    " ,I.DESCR60 AS ITEM_DESC " & vbCrLf & _
                                    " ,E.QTY_PICKED AS QTY_PICKED " & vbCrLf & _
                                    " ,E.QTY_SHIPPED AS QTY_SHIPPED " & vbCrLf & _
                                    " ,E.SHIP_DATE AS SHIP_DATE " & vbCrLf & _
                                    " ,F.SHIPTO_ID AS SHIPTO_ID " & vbCrLf & _
                                    " ,B.ISA_MACHINE_NO AS ISA_MACHINE_NO " & vbCrLf & _
                                    " ,B.ISA_WORK_ORDER_NO AS ISA_WORK_ORDER_NO " & vbCrLf & _
                                    " ,(" & vbCrLf & _
                                    "    SELECT LOC.DESCR AS DESCR " & vbCrLf & _
                                    "    FROM PS_LOCATION_TBL LOC" & vbCrLf & _
                                    "    WHERE LOC.SETID='MAIN1'" & vbCrLf & _
                                    "      AND LOC.EFF_STATUS = 'A'" & vbCrLf & _
                                    "      AND LOC.LOCATION = F.SHIPTO_ID " & vbCrLf & _
                                    "      AND LOC.EFFDT = (" & vbCrLf & _
                                    "                       SELECT MAX(LOC1.EFFDT)" & vbCrLf & _
                                    "                       FROM PS_LOCATION_TBL LOC1" & vbCrLf & _
                                    "                       WHERE LOC1.SETID = LOC.SETID" & vbCrLf & _
                                    "                         AND LOC1.LOCATION = LOC.LOCATION" & vbCrLf & _
                                    "                         AND LOC1.EFF_STATUS = LOC.EFF_STATUS" & vbCrLf & _
                                    "                         AND LOC1.EFFDT <= SYSDATE" & vbCrLf & _
                                    "                      )" & vbCrLf & _
                                    "      AND ROWNUM < 2" & vbCrLf & _
                                    "  ) AS SHIPTO_LOC" & vbCrLf & _
                                    " FROM " & vbCrLf & _
                                    "  PS_ISA_ORD_INTFC_H A" & vbCrLf & _
                                    " ,PS_ISA_ORD_INTFC_L B" & vbCrLf & _
                                    " ,(" & vbCrLf & _
                                    "   SELECT " & vbCrLf & _
                                    "    USR.ISA_USER_ID " & vbCrLf & _
                                    "   ,USR.FIRST_NAME_SRCH " & vbCrLf & _
                                    "   ,USR.LAST_NAME_SRCH " & vbCrLf & _
                                    "   ,USR.BUSINESS_UNIT " & vbCrLf & _
                                    "   ,USR.ISA_EMPLOYEE_ID " & vbCrLf & _
                                    "   ,USR.ISA_EMPLOYEE_NAME " & vbCrLf & _
                                    "   ,USR.ISA_EMPLOYEE_EMAIL " & vbCrLf & _
                                    "   FROM PS_ISA_USERS_TBL USR " & vbCrLf & _
                                    "   WHERE USR.BUSINESS_UNIT = '" & ord.BusinessUnit & "' " & vbCrLf & _
                                    "  ) C " & vbCrLf & _
                                    " ,PS_ISA_ORD_INTFC_O D" & vbCrLf & _
                                    " ,PS_MASTER_ITEM_TBL I" & vbCrLf & _
                                    " ,PS_SHIP_INF_INV E" & vbCrLf & _
                                    " ,(" & vbCrLf & _
                                    "   SELECT LD.ORDER_NO, LD.SHIPTO_ID, LD.LOAD_ID " & vbCrLf & _
                                    "   FROM PS_ISA_LOAD_INT LD " & vbCrLf & _
                                    "   WHERE LD.ORDER_NO = '" & ord.OrderNo & "' " & vbCrLf & _
                                    "     AND ROWNUM < 2 " & vbCrLf & _
                                    "  ) F " & vbCrLf & _
                                    " WHERE A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
                                    "   AND A.BUSINESS_UNIT_OM = '" & ord.BusinessUnit & "' " & vbCrLf & _
                                    "   AND A.ORDER_NO = '" & ord.OrderNo & "' " & vbCrLf & _
                                    "   AND '" & ord.BusinessUnit & "' = C.BUSINESS_UNIT (+) " & vbCrLf & _
                                    "   AND B.EMPLID = C.ISA_EMPLOYEE_ID (+) " & vbCrLf & _
                                    "   AND A.BUSINESS_UNIT_OM = D.BUSINESS_UNIT_OM " & vbCrLf & _
                                    "   AND A.ORDER_NO = D.ORDER_NO " & vbCrLf & _
                                    "   AND B.LINE_NBR = D.INTFC_LINE_NUM " & vbCrLf & _
                                    "   AND 'OM' = E.DEMAND_SOURCE " & vbCrLf & _
                                    "   AND A.BUSINESS_UNIT_OM = E.SOURCE_BUS_UNIT " & vbCrLf & _
                                    "   AND D.ORDER_NO = E.ORDER_NO " & vbCrLf & _
                                    "   AND D.ORDER_INT_LINE_NO = E.ORDER_INT_LINE_NO " & vbCrLf & _
                                    "   AND 'MAIN1' = I.SETID " & vbCrLf & _
                                    "   AND E.INV_ITEM_ID = I.INV_ITEM_ID " & vbCrLf & _
                                    "   AND E.SHIP_DATE IS NOT NULL " & vbCrLf & _
                                    "   AND A.ORDER_NO = F.ORDER_NO " & vbCrLf & _
                                    "   AND B.INV_ITEM_ID <> ' ' " & vbCrLf & _
                                    "   AND E.INV_ITEM_ID <> ' ' " & vbCrLf & _
                                    "   AND E.INV_ITEM_ID <> 'NSTK'" & vbCrLf & _
                                    "   AND E.QTY_SHIPPED > 0 " & vbCrLf & _
                                    "   AND NOT EXISTS (" & vbCrLf & _
                                    "                   SELECT 'X'" & vbCrLf & _
                                    "                   FROM PS_ISA_ORDSTAT_EML G" & vbCrLf & _
                                    "                   WHERE A.BUSINESS_UNIT_OM = G.BUSINESS_UNIT_OM" & vbCrLf & _
                                    "                     AND A.ORDER_NO = G.ORDER_NO" & vbCrLf & _
                                    "                     AND D.INTFC_LINE_NUM = G.LINE_NBR" & vbCrLf & _
                                    "                     AND E.ORDER_INT_LINE_NO = G.ORDER_INT_LINE_NO" & vbCrLf & _
                                    "                     AND E.DEMAND_LINE_NO = G.DEMAND_LINE_NO" & vbCrLf & _
                                    "                     AND G.ISA_ORDER_STATUS = '7'" & vbCrLf & _
                                    "                  )" & vbCrLf & _
                                    " UNION" & vbCrLf & _
                                    " SELECT " & vbCrLf & _
                                    "  A.BUSINESS_UNIT_OM AS BUSINESS_UNIT " & vbCrLf & _
                                    " ,A.ORDER_NO AS ORDER_NO " & vbCrLf & _
                                    " ,B.LINE_NBR AS LINE_NBR " & vbCrLf & _
                                    " ,E.ORDER_INT_LINE_NO AS ORDER_INT_LINE_NO " & vbCrLf & _
                                    " ,E.DEMAND_LINE_NO AS DEMAND_LINE_NO " & vbCrLf & _
                                    " ,B.EMPLID AS ISA_EMPLOYEE_ID " & vbCrLf & _
                                    " ,(C.FIRST_NAME_SRCH || ' ' || C.LAST_NAME_SRCH) AS ISA_EMPLOYEE_NAME " & vbCrLf & _
                                    " ,C.ISA_EMPLOYEE_EMAIL AS ISA_EMPLOYEE_EMAIL " & vbCrLf & _
                                    " ,' ' AS ITEM_ID " & vbCrLf & _
                                    " ,B.DESCR254 AS ITEM_DESC " & vbCrLf & _
                                    " ,E.QTY_PICKED AS QTY_PICKED " & vbCrLf & _
                                    " ,E.QTY_PICKED AS QTY_SHIPPED " & vbCrLf & _
                                    " ,E.SHIP_DTTM AS SHIP_DATE " & vbCrLf & _
                                    " ,F.SHIPTO_ID AS SHIPTO_ID " & vbCrLf & _
                                    " ,B.ISA_MACHINE_NO AS ISA_MACHINE_NO " & vbCrLf & _
                                    " ,B.ISA_WORK_ORDER_NO AS ISA_WORK_ORDER_NO " & vbCrLf & _
                                    " ,(" & vbCrLf & _
                                    "    SELECT LOC.DESCR AS DESCR " & vbCrLf & _
                                    "    FROM PS_LOCATION_TBL LOC" & vbCrLf & _
                                    "    WHERE LOC.SETID='MAIN1'" & vbCrLf & _
                                    "      AND LOC.EFF_STATUS = 'A'" & vbCrLf & _
                                    "      AND LOC.LOCATION = F.SHIPTO_ID" & vbCrLf & _
                                    "      AND LOC.EFFDT = (" & vbCrLf & _
                                    "                       SELECT MAX(LOC1.EFFDT)" & vbCrLf & _
                                    "                       FROM PS_LOCATION_TBL LOC1" & vbCrLf & _
                                    "                       WHERE LOC1.SETID = LOC.SETID" & vbCrLf & _
                                    "                         AND LOC1.LOCATION = LOC.LOCATION" & vbCrLf & _
                                    "                         AND LOC1.EFF_STATUS = LOC.EFF_STATUS" & vbCrLf & _
                                    "                         AND LOC1.EFFDT <= SYSDATE" & vbCrLf & _
                                    "                      )" & vbCrLf & _
                                    "      AND ROWNUM < 2" & vbCrLf & _
                                    "  ) AS SHIPTO_LOC" & vbCrLf & _
                                    " FROM " & vbCrLf & _
                                    "  PS_ISA_ORD_INTFC_H A" & vbCrLf & _
                                    " ,PS_ISA_ORD_INTFC_L B" & vbCrLf & _
                                    " ,(" & vbCrLf & _
                                    "   SELECT " & vbCrLf & _
                                    "    USR.ISA_USER_ID " & vbCrLf & _
                                    "   ,USR.FIRST_NAME_SRCH " & vbCrLf & _
                                    "   ,USR.LAST_NAME_SRCH " & vbCrLf & _
                                    "   ,USR.BUSINESS_UNIT " & vbCrLf & _
                                    "   ,USR.ISA_EMPLOYEE_ID " & vbCrLf & _
                                    "   ,USR.ISA_EMPLOYEE_NAME " & vbCrLf & _
                                    "   ,USR.ISA_EMPLOYEE_EMAIL " & vbCrLf & _
                                    "   FROM PS_ISA_USERS_TBL USR " & vbCrLf & _
                                    "   WHERE USR.BUSINESS_UNIT = '" & ord.BusinessUnit & "' " & vbCrLf & _
                                    "  ) C " & vbCrLf & _
                                    " ,PS_ISA_PICKING_INT E" & vbCrLf & _
                                    " ,(" & vbCrLf & _
                                    "   SELECT LD.ORDER_NO, LD.SHIPTO_ID, LD.LOAD_ID " & vbCrLf & _
                                    "   FROM PS_ISA_LOAD_INT LD " & vbCrLf & _
                                    "   WHERE LD.ORDER_NO = '" & ord.OrderNo & "' " & vbCrLf & _
                                    "     AND ROWNUM < 2 " & vbCrLf & _
                                    "  ) F " & vbCrLf & _
                                    " WHERE A.ISA_IDENTIFIER = B.ISA_PARENT_IDENT" & vbCrLf & _
                                    "   AND A.BUSINESS_UNIT_OM = '" & ord.BusinessUnit & "' " & vbCrLf & _
                                    "   AND A.ORDER_NO = '" & ord.OrderNo & "' " & vbCrLf & _
                                    "   AND '" & ord.BusinessUnit & "' = C.BUSINESS_UNIT (+) " & vbCrLf & _
                                    "   AND B.EMPLID = C.ISA_EMPLOYEE_ID (+) " & vbCrLf & _
                                    "   AND 'OM' = E.DEMAND_SOURCE " & vbCrLf & _
                                    "   AND A.BUSINESS_UNIT_OM = E.SOURCE_BUS_UNIT " & vbCrLf & _
                                    "   AND A.ORDER_NO = E.ORDER_NO " & vbCrLf & _
                                    "   AND B.LINE_NBR = E.ORDER_INT_LINE_NO " & vbCrLf & _
                                    "   AND E.SHIP_DTTM IS NOT NULL " & vbCrLf & _
                                    "   AND A.ORDER_NO = F.ORDER_NO " & vbCrLf & _
                                    "   AND B.INV_ITEM_ID = ' ' " & vbCrLf & _
                                    "   AND E.INV_ITEM_ID = 'NSTK'" & vbCrLf & _
                                    "   AND E.QTY_PICKED > 0 " & vbCrLf & _
                                    "   AND NOT EXISTS (" & vbCrLf & _
                                    "                   SELECT 'X'" & vbCrLf & _
                                    "                   FROM PS_ISA_ORDSTAT_EML G" & vbCrLf & _
                                    "                   WHERE A.BUSINESS_UNIT_OM = G.BUSINESS_UNIT_OM" & vbCrLf & _
                                    "                     AND A.ORDER_NO = G.ORDER_NO" & vbCrLf & _
                                    "                     AND B.LINE_NBR = G.LINE_NBR" & vbCrLf & _
                                    "                     AND E.ORDER_INT_LINE_NO = G.ORDER_INT_LINE_NO" & vbCrLf & _
                                    "                     AND E.DEMAND_LINE_NO = G.DEMAND_LINE_NO" & vbCrLf & _
                                    "                     AND G.ISA_ORDER_STATUS = '7'" & vbCrLf & _
                                    "                  )" & vbCrLf & _
                                    ") X" & vbCrLf & _
                                    "ORDER BY X.BUSINESS_UNIT, X.ORDER_NO, X.ISA_EMPLOYEE_ID, X.LINE_NBR " & vbCrLf & _
                                    ""

                Dim cmd As OleDbCommand = m_oraCN.CreateCommand
                cmd.CommandText = sql
                cmd.CommandType = CommandType.Text

                Dim rdr As OleDbDataReader = Nothing

                Try
                    rdr = cmd.ExecuteReader
                Catch ex As Exception
                End Try

                If Not (rdr Is Nothing) Then
                    Dim empId As String = "~initialEmpId"
                    Dim sName As String = ""
                    Dim sEmailAdd As String = ""
                    Dim bIsFabOrder As Boolean = False
                    Dim shipToLocName As String = ""
                    While rdr.Read
                        ' create a new myOrder class everytime employee Id changes for this order
                        '   because we're going to send separate emails for each employee/user
                        If (empId.Trim.ToUpper <> CStr(rdr("ISA_EMPLOYEE_ID"))) Then
                            myOrder = New orderHdr
                            myOrder.BusinessUnit = CStr(rdr("BUSINESS_UNIT")).Trim.ToUpper
                            myOrder.OrderNo = CStr(rdr("ORDER_NO")).Trim.ToUpper
                            myOrder.RecipientId = CStr(rdr("ISA_EMPLOYEE_ID")).Trim.ToUpper
                            sName = ""
                            Try
                                sName = CStr(rdr("ISA_EMPLOYEE_NAME")).Trim
                                If (sName Is Nothing) Then
                                    sName = ""
                                End If
                            Catch ex As Exception
                            End Try
                            myOrder.RecipientName = sName
                            sEmailAdd = ""
                            Try
                                sEmailAdd = CStr(rdr("ISA_EMPLOYEE_EMAIL")).Trim
                                If (sEmailAdd Is Nothing) Then
                                    sEmailAdd = ""
                                End If
                            Catch ex As Exception
                            End Try
                            myOrder.RecipientEmailAddress = sEmailAdd
                            myOrder.ShipToLocation = CStr(rdr("SHIPTO_ID"))
                            bIsFabOrder = False
                            Try
                                bIsFabOrder = (CStr(rdr("ISA_MACHINE_NO")).Trim.ToUpper = flag_FAB_ORDER)
                            Catch ex As Exception
                            End Try
                            If Not myOrder.IsFabOrder And _
                               bIsFabOrder Then
                                myOrder.IsFabOrder = bIsFabOrder
                            End If
                            shipToLocName = ""
                            Try
                                shipToLocName = CStr(rdr("SHIPTO_LOC")).Trim
                                If (sName Is Nothing) Then
                                    shipToLocName = ""
                                End If
                            Catch ex As Exception
                            End Try
                            myOrder.ShipToLocationName = shipToLocName
                            empId = myOrder.RecipientId
                            arr.Add(myOrder)
                        End If
                        ' check/update bu
                        If myOrder.BusinessUnit.Length = 0 Then
                            myOrder.BusinessUnit = CStr(rdr("BUSINESS_UNIT")).Trim.ToUpper
                        End If
                        ' check/update order number
                        If myOrder.OrderNo.Length = 0 Then
                            myOrder.OrderNo = CStr(rdr("ORDER_NO")).Trim.ToUpper
                        End If
                        ' check/update recipient Id
                        If myOrder.RecipientId.Length = 0 Then
                            myOrder.RecipientId = CStr(rdr("ISA_EMPLOYEE_ID")).Trim.ToUpper
                        End If
                        ' check/update recipient name
                        If myOrder.RecipientName.Length = 0 Then
                            sName = ""
                            Try
                                sName = CStr(rdr("ISA_EMPLOYEE_NAME")).Trim
                                If (sName Is Nothing) Then
                                    sName = ""
                                End If
                            Catch ex As Exception
                            End Try
                            myOrder.RecipientName = sName
                        End If
                        ' check/update recipient email address
                        If myOrder.RecipientEmailAddress.Length = 0 Then
                            sEmailAdd = ""
                            Try
                                sEmailAdd = CStr(rdr("ISA_EMPLOYEE_EMAIL")).Trim
                                If (sEmailAdd Is Nothing) Then
                                    sEmailAdd = ""
                                End If
                            Catch ex As Exception
                            End Try
                            myOrder.RecipientEmailAddress = sEmailAdd
                        End If
                        ' check/update ship to location
                        If myOrder.ShipToLocation.Length = 0 Then
                            myOrder.ShipToLocation = CStr(rdr("SHIPTO_ID"))
                        End If
                        ' check/update if this is a FAB order (any line flagged as FAB)
                        bIsFabOrder = False
                        Try
                            bIsFabOrder = (CStr(rdr("ISA_MACHINE_NO")).Trim.ToUpper = flag_FAB_ORDER)
                        Catch ex As Exception
                        End Try
                        If Not myOrder.IsFabOrder And _
                           bIsFabOrder Then
                            myOrder.IsFabOrder = bIsFabOrder
                        End If
                        ' check/update ship
                        If myOrder.ShipToLocationName.Length = 0 Then
                            shipToLocName = ""
                            Try
                                shipToLocName = CStr(rdr("SHIPTO_LOC")).Trim
                                If (sName Is Nothing) Then
                                    shipToLocName = ""
                                End If
                            Catch ex As Exception
                            End Try
                            myOrder.ShipToLocationName = shipToLocName
                        End If
                        ordLine = New orderLine
                        ordLine.LineNo = CInt(rdr("LINE_NBR"))
                        ordLine.ItemId = CStr(rdr("ITEM_ID"))
                        ordLine.ItemDesc = CStr(rdr("ITEM_DESC"))
                        ordLine.ItemQuantity = CDec(rdr("QTY_SHIPPED"))
                        ordLine.ShippedDate = CDate(CStr(rdr("SHIP_DATE")))
                        ordLine.DemandLineNo = CInt(rdr("DEMAND_LINE_NO"))
                        ordLine.OrderIntLineNo = CInt(rdr("ORDER_INT_LINE_NO"))
                        ordLine.WorkOrderNo = CStr(rdr("ISA_WORK_ORDER_NO"))
                        '' check if item Id and line number combination already exists
                        ''   if it is, just add the quantity regardless if different demand/picking line numbers
                        'Dim bIsLineItemFound As Boolean = False
                        'If myOrder.OrderLines.Count > 0 Then
                        '    For Each i As orderLine In myOrder.OrderLines
                        '        If i.LineNo = ordLine.LineNo And _
                        '           i.ItemId = ordLine.ItemId Then
                        '            ' add this quantity
                        '            i.ItemQuantity += ordLine.ItemQuantity
                        '            ' update ship date, demand line and order line numbers if ship date is later
                        '            If i.ShippedDate.Length = 0 Then
                        '                i.ShippedDate = ordLine.ShippedDate
                        '                i.DemandLineNo = ordLine.DemandLineNo
                        '                i.OrderIntLineNo = ordLine.OrderIntLineNo
                        '            Else
                        '                If Not IsDate(i.ShippedDate) Then
                        '                    i.ShippedDate = ordLine.ShippedDate
                        '                    i.DemandLineNo = ordLine.DemandLineNo
                        '                    i.OrderIntLineNo = ordLine.OrderIntLineNo
                        '                Else
                        '                    If CDate(i.ShippedDate) < CDate(ordLine.ShippedDate) Then
                        '                        i.ShippedDate = ordLine.ShippedDate
                        '                        i.DemandLineNo = ordLine.DemandLineNo
                        '                        i.OrderIntLineNo = ordLine.OrderIntLineNo
                        '                    End If
                        '                End If
                        '            End If
                        '            ' set flag
                        '            '   so below code won't re-add this item/line number
                        '            bIsLineItemFound = True
                        '            Exit For
                        '        End If
                        '    Next
                        'End If
                        'If Not bIsLineItemFound Then
                        '    ordLine.ParentOrder = myOrder
                        '    myOrder.OrderLines.Add(ordLine)
                        'End If
                        ordLine.ParentOrder = myOrder
                        myOrder.OrderLines.Add(ordLine)
                    End While
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

                m_oraCN.Close()

                '// send email
                '//     and flag order/order lines
                If arr.Count > 0 Then
                    For Each o As orderHdr In arr
                        If o.OrderLines.Count > 0 Then
                            ' (1) don't send notificaton if Order was of type "FAB"
                            '     but still flag MR
                            If Not o.IsFabOrder Then
                                If o.OrderNo.Substring(0, 4) = "NY0A" Then
                                    ' repick order
                                    '   include emails in BCC for repick orders
                                    'Me.sendEmailNotification(o, "", Me.bccForAll & Me.bccForNY0A)
                                    Me.sendEmailNotification(o, Me.bccForNY0A, Me.bccForAll & Me.bccForNY0A)
                                Else
                                    ' other/regular
                                    Me.sendEmailNotification(o, "", Me.bccForAll)
                                End If
                                m_logger.WriteVerboseLog("generated email notification for order " & o.OrderNo & " ")
                            Else
                                If o.OrderNo.Substring(0, 4) = "NY0A" Then
                                    ' repick order
                                    '   STILL generate email to BCC for repick orders
                                    'Me.sendEmailNotification(o, "", Me.bccForAll & Me.bccForNY0A, True, True)
                                    Me.sendEmailNotification(o, Me.bccForNY0A, Me.bccForAll & Me.bccForNY0A, True, True)
                                    m_logger.WriteVerboseLog("generated email notification for order " & o.OrderNo & " but ONLY for " & Me.bccForNY0A & " and not for customer due that it's of type FAB.")
                                Else
                                    ' just log that we didn't generate the email
                                    m_logger.WriteVerboseLog("skipped email generation for order " & o.OrderNo & " due that it's of type FAB.")
                                End If
                            End If
                            Me.flagOrderAsProcessed(o)
                            m_logger.WriteVerboseLog("flagged order " & o.OrderNo & " as processed.")
                        End If
                    Next
                End If

                myOrder = Nothing
            Next
        End If

        m_appSettings = Nothing
        m_logger = Nothing
        m_oraCN = Nothing
    End Sub

#End Region

#Region " ICanCheckNSTKOrderShippedStatus Implementations "

    Public Sub checkNSTKOrderShippedStatus(ByVal sender As Object, ByVal e As orderShippedStatusEventArgs) Implements ICanCheckNSTKOrderShippedStatus.checkNSTKOrderShippedStatus

    End Sub

    Public Sub sendNSTKOrderShippedStatusEmail(ByVal sender As Object, ByVal e As orderShippedStatusEventArgs) Implements ICanCheckNSTKOrderShippedStatus.sendNSTKOrderShippedStatusEmail
        m_oraCN = e.oledbCN
        m_logger = e.Logger
        m_appSettings = e.AppSettings

        ' get link for scanned document
        Dim s As String = ""
        Try
            s = CStr(m_appSettings(Me.APP_SETTING_SCANNED_DOC_LINK))
            If (s Is Nothing) Then
                s = ""
            End If
        Catch ex As Exception
        End Try
        If s.Length > 0 Then
            m_scannedDocLinkURL = s
        End If

        Const flag_FAB_ORDER As String = "FAB"

        Dim dtStart As DateTime = Me.StartDate
        Dim dtEnd As DateTime = Now
        Dim arrOrders As ArrayList = Me.getShippedNSTKOrders(Me.SiteId, _
                                                             dtStart, _
                                                             dtEnd)
        m_logger.WriteVerboseLog("checking " & arrOrders.Count.ToString & " NON-STOCK order(s)/order line(s) ...")
        If arrOrders.Count > 0 Then
            For Each ord As changedOrder In arrOrders

                Dim arr As New ArrayList
                Dim myOrder As orderHdr = Nothing
                Dim ordLine As orderLine = Nothing

                If m_oraCN.State <> ConnectionState.Closed Then
                    m_oraCN.Close()
                End If
                m_oraCN.Open()

                '// grab order data
                Dim sql As String = "" & _
                                    "SELECT " & vbCrLf & _
                                    " X.BUSINESS_UNIT " & vbCrLf & _
                                    ",X.ORDER_NO " & vbCrLf & _
                                    ",X.LINE_NBR " & vbCrLf & _
                                    ",X.ORDER_INT_LINE_NO " & vbCrLf & _
                                    ",X.DEMAND_LINE_NO " & vbCrLf & _
                                    ",X.ISA_EMPLOYEE_ID " & vbCrLf & _
                                    ",X.ISA_EMPLOYEE_NAME " & vbCrLf & _
                                    ",X.ISA_EMPLOYEE_EMAIL " & vbCrLf & _
                                    ",X.ITEM_ID " & vbCrLf & _
                                    ",X.ITEM_DESC " & vbCrLf & _
                                    ",X.QTY_PICKED " & vbCrLf & _
                                    ",X.QTY_SHIPPED " & vbCrLf & _
                                    ",X.SHIP_DATE " & vbCrLf & _
                                    ",X.SHIPTO_ID " & vbCrLf & _
                                    ",X.ISA_MACHINE_NO " & vbCrLf & _
                                    ",X.ISA_WORK_ORDER_NO " & vbCrLf & _
                                    ",X.SHIPTO_LOC " & vbCrLf & _
                                    "FROM " & vbCrLf & _
                                    "(" & vbCrLf & _
                                    " SELECT " & vbCrLf & _
                                    "  I.BUSINESS_UNIT_OM AS BUSINESS_UNIT " & vbCrLf & _
                                    " ,A.REQ_ID AS ORDER_NO " & vbCrLf & _
                                    " ,B.LINE_NBR AS LINE_NBR " & vbCrLf & _
                                    " ,0 AS ORDER_INT_LINE_NO " & vbCrLf & _
                                    " ,0 AS DEMAND_LINE_NO " & vbCrLf & _
                                    " ,B.ISA_EMPLOYEE_ID AS ISA_EMPLOYEE_ID " & vbCrLf & _
                                    " ,(C.FIRST_NAME_SRCH || ' ' || C.LAST_NAME_SRCH) AS ISA_EMPLOYEE_NAME " & vbCrLf & _
                                    " ,C.ISA_EMPLOYEE_EMAIL AS ISA_EMPLOYEE_EMAIL " & vbCrLf & _
                                    " ,' ' AS ITEM_ID " & vbCrLf & _
                                    " ,B.DESCR254_MIXED AS ITEM_DESC " & vbCrLf & _
                                    " ,H.QTY_LN_ACCPT_VUOM AS QTY_PICKED " & vbCrLf & _
                                    " ,H.QTY_LN_ACCPT_VUOM AS QTY_SHIPPED " & vbCrLf & _
                                    " ,H.RECEIPT_DT AS SHIP_DATE " & vbCrLf & _
                                    " ,H.SHIPTO_ID AS SHIPTO_ID " & vbCrLf & _
                                    " ,B.ISA_MACHINE_NO AS ISA_MACHINE_NO " & vbCrLf & _
                                    " ,B.ISA_WORK_ORDER_NO AS ISA_WORK_ORDER_NO " & vbCrLf & _
                                    " ,(" & vbCrLf & _
                                    "    SELECT LOC.DESCR AS DESCR " & vbCrLf & _
                                    "    FROM PS_LOCATION_TBL LOC" & vbCrLf & _
                                    "    WHERE LOC.SETID='MAIN1'" & vbCrLf & _
                                    "      AND LOC.EFF_STATUS = 'A'" & vbCrLf & _
                                    "      AND LOC.LOCATION = H.SHIPTO_ID " & vbCrLf & _
                                    "      AND LOC.EFFDT = (" & vbCrLf & _
                                    "                       SELECT MAX(LOC1.EFFDT)" & vbCrLf & _
                                    "                       FROM PS_LOCATION_TBL LOC1" & vbCrLf & _
                                    "                       WHERE LOC1.SETID = LOC.SETID" & vbCrLf & _
                                    "                         AND LOC1.LOCATION = LOC.LOCATION" & vbCrLf & _
                                    "                         AND LOC1.EFF_STATUS = LOC.EFF_STATUS" & vbCrLf & _
                                    "                         AND LOC1.EFFDT <= SYSDATE" & vbCrLf & _
                                    "                      )" & vbCrLf & _
                                    "      AND ROWNUM < 2" & vbCrLf & _
                                    "  ) AS SHIPTO_LOC" & vbCrLf & _
                                    " FROM " & vbCrLf & _
                                    "  PS_REQ_HDR A " & vbCrLf & _
                                    " ,PS_REQ_LINE B " & vbCrLf & _
                                    " ,(" & vbCrLf & _
                                    "   SELECT " & vbCrLf & _
                                    "    USR.ISA_USER_ID " & vbCrLf & _
                                    "   ,USR.FIRST_NAME_SRCH " & vbCrLf & _
                                    "   ,USR.LAST_NAME_SRCH " & vbCrLf & _
                                    "   ,USR.BUSINESS_UNIT " & vbCrLf & _
                                    "   ,USR.ISA_EMPLOYEE_ID " & vbCrLf & _
                                    "   ,USR.ISA_EMPLOYEE_NAME " & vbCrLf & _
                                    "   ,USR.ISA_EMPLOYEE_EMAIL " & vbCrLf & _
                                    "   FROM PS_ISA_USERS_TBL USR " & vbCrLf & _
                                    "   WHERE USR.BUSINESS_UNIT = '" & ord.BusinessUnit & "' " & vbCrLf & _
                                    "  ) C " & vbCrLf & _
                                    " ,(" & vbCrLf & _
                                    "   SELECT " & vbCrLf & _
                                    "    H1.BUSINESS_UNIT" & vbCrLf & _
                                    "   ,H1.REQ_ID" & vbCrLf & _
                                    "   ,H1.REQ_LINE_NBR" & vbCrLf & _
                                    "   ,SUM(H1.QTY_PO) AS QTY_PO" & vbCrLf & _
                                    "   ,SUM(NVL(H2.QTY_LN_ACCPT_VUOM,0)) AS QTY_LN_ACCPT_VUOM" & vbCrLf & _
                                    "   ,MAX(H3.RECEIPT_DT) AS RECEIPT_DT" & vbCrLf & _
                                    "   ,(" & vbCrLf & _
                                    "     SELECT H11.LOCATION" & vbCrLf & _
                                    "     FROM PS_PO_LINE_DISTRIB H11" & vbCrLf & _
                                    "     WHERE H11.BUSINESS_UNIT = H1.BUSINESS_UNIT" & vbCrLf & _
                                    "       AND H11.REQ_ID = H1.REQ_ID" & vbCrLf & _
                                    "       AND H11.REQ_LINE_NBR = H1.REQ_LINE_NBR" & vbCrLf & _
                                    "       AND H11.DISTRIB_LN_STATUS <> 'X'" & vbCrLf & _
                                    "       AND ROWNUM < 2" & vbCrLf & _
                                    "     GROUP BY H11.LOCATION " & vbCrLf & _
                                    "    ) AS SHIPTO_ID" & vbCrLf & _
                                    "   FROM PS_PO_LINE_DISTRIB H1" & vbCrLf & _
                                    "   ,PS_RECV_LN H2" & vbCrLf & _
                                    "   ,PS_RECV_HDR H3" & vbCrLf & _
                                    "   WHERE H1.BUSINESS_UNIT = 'ISA00'" & vbCrLf & _
                                    "     AND H1.REQ_ID = '" & ord.OrderNo & "' " & vbCrLf & _
                                    "     AND H1.DISTRIB_LN_STATUS <> 'X'" & vbCrLf & _
                                    "     AND H1.BUSINESS_UNIT = H2.BUSINESS_UNIT_PO (+)" & vbCrLf & _
                                    "     AND H1.PO_ID = H2.PO_ID (+)" & vbCrLf & _
                                    "     AND H1.LINE_NBR = H2.LINE_NBR (+)" & vbCrLf & _
                                    "     AND H2.BUSINESS_UNIT = H3.BUSINESS_UNIT (+)" & vbCrLf & _
                                    "     AND H2.RECEIVER_ID = H3.RECEIVER_ID (+)" & vbCrLf & _
                                    "   GROUP BY " & vbCrLf & _
                                    "    H1.BUSINESS_UNIT" & vbCrLf & _
                                    "   ,H1.REQ_ID" & vbCrLf & _
                                    "   ,H1.REQ_LINE_NBR" & vbCrLf & _
                                    "   ORDER BY " & vbCrLf & _
                                    "    H1.BUSINESS_UNIT" & vbCrLf & _
                                    "   ,H1.REQ_ID" & vbCrLf & _
                                    "   ,H1.REQ_LINE_NBR" & vbCrLf & _
                                    "  ) H" & vbCrLf & _
                                    " ,PS_ISA_ORD_INTFC_H I " & vbCrLf & _
                                    " WHERE A.BUSINESS_UNIT = 'ISA00' " & vbCrLf & _
                                    "   AND A.REQ_ID = '" & ord.OrderNo & "' " & vbCrLf & _
                                    "   AND A.BUSINESS_UNIT = B.BUSINESS_UNIT " & vbCrLf & _
                                    "   AND A.REQ_ID = B.REQ_ID " & vbCrLf & _
                                    "   AND '" & ord.BusinessUnit & "' = I.BUSINESS_UNIT_OM " & vbCrLf & _
                                    "   AND A.REQ_ID = I.ORDER_NO " & vbCrLf & _
                                    "   AND '" & ord.BusinessUnit & "' = C.BUSINESS_UNIT (+) " & vbCrLf & _
                                    "   AND B.ISA_EMPLOYEE_ID = C.ISA_EMPLOYEE_ID (+) " & vbCrLf & _
                                    "   AND A.REQ_ID = H.REQ_ID " & vbCrLf & _
                                    "   AND B.LINE_NBR = H.REQ_LINE_NBR " & vbCrLf & _
                                    "   AND NOT EXISTS (" & vbCrLf & _
                                    "                   SELECT 'X'" & vbCrLf & _
                                    "                   FROM PS_ISA_ORDSTAT_EML G" & vbCrLf & _
                                    "                   WHERE I.BUSINESS_UNIT_OM = G.BUSINESS_UNIT_OM" & vbCrLf & _
                                    "                     AND A.REQ_ID = G.ORDER_NO" & vbCrLf & _
                                    "                     AND B.LINE_NBR = G.LINE_NBR" & vbCrLf & _
                                    "                     AND 0 = G.ORDER_INT_LINE_NO" & vbCrLf & _
                                    "                     AND 0 = G.DEMAND_LINE_NO" & vbCrLf & _
                                    "                     AND G.ISA_ORDER_STATUS = '7'" & vbCrLf & _
                                    "                  )" & vbCrLf & _
                                    ") X" & vbCrLf & _
                                    "ORDER BY X.BUSINESS_UNIT, X.ORDER_NO, X.ISA_EMPLOYEE_ID, X.LINE_NBR " & vbCrLf & _
                                    ""

                Dim cmd As OleDbCommand = m_oraCN.CreateCommand
                cmd.CommandText = sql
                cmd.CommandType = CommandType.Text

                Dim rdr As OleDbDataReader = Nothing

                Try
                    rdr = cmd.ExecuteReader
                Catch ex As Exception
                End Try

                If Not (rdr Is Nothing) Then
                    Dim empId As String = "~initialEmpId"
                    Dim sName As String = ""
                    Dim sEmailAdd As String = ""
                    Dim bIsFabOrder As Boolean = False
                    Dim shipToLocName As String = ""
                    While rdr.Read
                        ' create a new myOrder class everytime employee Id changes for this order
                        '   because we're going to send separate emails for each employee/user
                        If (empId.Trim.ToUpper <> CStr(rdr("ISA_EMPLOYEE_ID"))) Then
                            myOrder = New orderHdr
                            myOrder.BusinessUnit = CStr(rdr("BUSINESS_UNIT")).Trim.ToUpper
                            myOrder.OrderNo = CStr(rdr("ORDER_NO")).Trim.ToUpper
                            myOrder.RecipientId = CStr(rdr("ISA_EMPLOYEE_ID")).Trim.ToUpper
                            sName = ""
                            Try
                                sName = CStr(rdr("ISA_EMPLOYEE_NAME")).Trim
                                If (sName Is Nothing) Then
                                    sName = ""
                                End If
                            Catch ex As Exception
                            End Try
                            myOrder.RecipientName = sName
                            sEmailAdd = ""
                            Try
                                sEmailAdd = CStr(rdr("ISA_EMPLOYEE_EMAIL")).Trim
                                If (sEmailAdd Is Nothing) Then
                                    sEmailAdd = ""
                                End If
                            Catch ex As Exception
                            End Try
                            myOrder.RecipientEmailAddress = sEmailAdd
                            myOrder.ShipToLocation = CStr(rdr("SHIPTO_ID"))
                            bIsFabOrder = False
                            Try
                                bIsFabOrder = (CStr(rdr("ISA_MACHINE_NO")).Trim.ToUpper = flag_FAB_ORDER)
                            Catch ex As Exception
                            End Try
                            If Not myOrder.IsFabOrder And _
                               bIsFabOrder Then
                                myOrder.IsFabOrder = bIsFabOrder
                            End If
                            shipToLocName = ""
                            Try
                                shipToLocName = CStr(rdr("SHIPTO_LOC")).Trim
                                If (sName Is Nothing) Then
                                    shipToLocName = ""
                                End If
                            Catch ex As Exception
                            End Try
                            myOrder.ShipToLocationName = shipToLocName
                            empId = myOrder.RecipientId
                            arr.Add(myOrder)
                        End If
                        ' check/update bu
                        If myOrder.BusinessUnit.Length = 0 Then
                            myOrder.BusinessUnit = CStr(rdr("BUSINESS_UNIT")).Trim.ToUpper
                        End If
                        ' check/update order number
                        If myOrder.OrderNo.Length = 0 Then
                            myOrder.OrderNo = CStr(rdr("ORDER_NO")).Trim.ToUpper
                        End If
                        ' check/update recipient Id
                        If myOrder.RecipientId.Length = 0 Then
                            myOrder.RecipientId = CStr(rdr("ISA_EMPLOYEE_ID")).Trim.ToUpper
                        End If
                        ' check/update recipient name
                        If myOrder.RecipientName.Length = 0 Then
                            sName = ""
                            Try
                                sName = CStr(rdr("ISA_EMPLOYEE_NAME")).Trim
                                If (sName Is Nothing) Then
                                    sName = ""
                                End If
                            Catch ex As Exception
                            End Try
                            myOrder.RecipientName = sName
                        End If
                        ' check/update recipient email address
                        If myOrder.RecipientEmailAddress.Length = 0 Then
                            sEmailAdd = ""
                            Try
                                sEmailAdd = CStr(rdr("ISA_EMPLOYEE_EMAIL")).Trim
                                If (sEmailAdd Is Nothing) Then
                                    sEmailAdd = ""
                                End If
                            Catch ex As Exception
                            End Try
                            myOrder.RecipientEmailAddress = sEmailAdd
                        End If
                        ' check/update ship to location
                        If myOrder.ShipToLocation.Length = 0 Then
                            myOrder.ShipToLocation = CStr(rdr("SHIPTO_ID"))
                        End If
                        ' check/update if this is a FAB order (any line flagged as FAB)
                        bIsFabOrder = False
                        Try
                            bIsFabOrder = (CStr(rdr("ISA_MACHINE_NO")).Trim.ToUpper = flag_FAB_ORDER)
                        Catch ex As Exception
                        End Try
                        If Not myOrder.IsFabOrder And _
                           bIsFabOrder Then
                            myOrder.IsFabOrder = bIsFabOrder
                        End If
                        ' check/update ship to location
                        If myOrder.ShipToLocationName.Length = 0 Then
                            shipToLocName = ""
                            Try
                                shipToLocName = CStr(rdr("SHIPTO_LOC")).Trim
                                If (sName Is Nothing) Then
                                    shipToLocName = ""
                                End If
                            Catch ex As Exception
                            End Try
                            myOrder.ShipToLocationName = shipToLocName
                        End If
                        ordLine = New orderLine
                        ordLine.LineNo = CInt(rdr("LINE_NBR"))
                        ordLine.ItemId = CStr(rdr("ITEM_ID"))
                        ordLine.ItemDesc = CStr(rdr("ITEM_DESC"))
                        Try
                            ordLine.ItemQuantity = CDec(rdr("QTY_SHIPPED"))
                        Catch ex As Exception
                        End Try
                        Try
                            ordLine.ShippedDate = CDate(CStr(rdr("SHIP_DATE")))
                        Catch ex As Exception
                        End Try
                        ordLine.DemandLineNo = CInt(rdr("DEMAND_LINE_NO"))
                        ordLine.OrderIntLineNo = CInt(rdr("ORDER_INT_LINE_NO"))
                        ordLine.WorkOrderNo = CStr(rdr("ISA_WORK_ORDER_NO"))
                        ordLine.ParentOrder = myOrder
                        myOrder.OrderLines.Add(ordLine)
                    End While
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

                m_oraCN.Close()

                '// send email
                '//     and flag order/order lines
                If arr.Count > 0 Then
                    For Each o As orderHdr In arr
                        If o.OrderLines.Count > 0 Then
                            If o.isOrderFullyShipped Then
                                ' don't send notificaton if Order was of type "FAB"
                                '   (1) we create receipts on our system that we received the non-stock material BUT will be sending it to FAB 
                                '       shop instead of the target school.  But since its out of SDI's hand, it will be considered as shipped.
                                '   but still flag MR
                                If Not o.IsFabOrder Then
                                    ' DO NOT show proof of delivery link since no docs will be available for NON-STOCK orders only
                                    If o.OrderNo.Substring(0, 4) = "NY0A" Then
                                        ' repick order, so include email addresses for NY0A
                                        'Me.sendEmailNotification(o, "", Me.bccForAll & Me.bccForNSTK & Me.bccForNY0A, False)
                                        Me.sendEmailNotification(o, Me.bccForNSTK & Me.bccForNY0A, Me.bccForAll & Me.bccForNSTK & Me.bccForNY0A, False)
                                    Else
                                        ' other/regular
                                        'Me.sendEmailNotification(o, "", Me.bccForAll & Me.bccForNSTK, False)
                                        Me.sendEmailNotification(o, Me.bccForNSTK, Me.bccForAll & Me.bccForNSTK, False)
                                    End If
                                    m_logger.WriteVerboseLog("generated email notification for NON-STOCK order " & o.OrderNo & " ")
                                Else
                                    If o.OrderNo.Substring(0, 4) = "NY0A" Then
                                        ' repick order
                                        '   STILL generate email to BCC for repick orders
                                        'Me.sendEmailNotification(o, "", Me.bccForAll & Me.bccForNY0A, False, True)
                                        Me.sendEmailNotification(o, Me.bccForNY0A, Me.bccForAll & Me.bccForNY0A, False, True)
                                        m_logger.WriteVerboseLog("generated email notification for order " & o.OrderNo & " but ONLY for " & Me.bccForNY0A & " and not for customer due that it's of type FAB.")
                                    Else
                                        ' just log that process didn't generate the email
                                        m_logger.WriteVerboseLog("skipped email generation for NON-STOCK order " & o.OrderNo & " due that it's of type FAB.")
                                    End If
                                End If
                                Me.flagOrderAsProcessed(o)
                                m_logger.WriteVerboseLog("flagged order " & o.OrderNo & " as processed.")
                            Else
                                m_logger.WriteVerboseLog("order " & o.OrderNo & " was not fully shipped.")
                            End If
                        End If
                    Next
                End If

                myOrder = Nothing
            Next
        End If

        m_appSettings = Nothing
        m_logger = Nothing
        m_oraCN = Nothing
    End Sub

#End Region

    Private Function getShippedOrders(ByVal bu As String, _
                                      ByVal dtStart As DateTime, _
                                      ByVal dtEnd As DateTime) As ArrayList
        Const ordStatusLog_SHIPPED As String = "6"
        Dim arr As New ArrayList

        If m_oraCN.State <> ConnectionState.Closed Then
            m_oraCN.Close()
        End If
        m_oraCN.Open()

        Dim sql As String = "" & vbCrLf & _
                            "SELECT " & vbCrLf & _
                            " A.ORDER_NO " & vbCrLf & _
                            ",A.BUSINESS_UNIT_OM " & vbCrLf & _
                            "FROM PS_ISAORDSTATUSLOG A" & vbCrLf & _
                            "WHERE (A.DTTM_STAMP BETWEEN TO_DATE('" & dtStart.ToString("MM/dd/yyyy HH:mm:ss") & "','MM/DD/YYYY HH24:MI:SS') AND TO_DATE('" & dtEnd.ToString("MM/dd/yyyy HH:mm:ss") & "','MM/DD/YYYY HH24:MI:SS')) " & vbCrLf & _
                            "  AND A.BUSINESS_UNIT_OM = '" & bu & "' " & vbCrLf & _
                            "  AND A.ISA_ORDER_STATUS = '" & ordStatusLog_SHIPPED & "' " & vbCrLf & _
                            "GROUP BY A.ORDER_NO, A.BUSINESS_UNIT_OM " & vbCrLf & _
                            ""
        Dim cmd As OleDbCommand = m_oraCN.CreateCommand
        cmd.CommandText = sql
        cmd.CommandType = CommandType.Text

        Dim rdr As OleDbDataReader = Nothing

        Try
            rdr = cmd.ExecuteReader
        Catch ex As Exception
        End Try

        If Not (rdr Is Nothing) Then
            Dim sBU As String = ""
            Dim sOrder As String = ""
            While rdr.Read
                sBU = ""
                Try
                    sBU = CStr(rdr("BUSINESS_UNIT_OM")).Trim.ToUpper
                Catch ex As Exception
                End Try
                sOrder = ""
                Try
                    sOrder = CStr(rdr("ORDER_NO")).Trim.ToUpper
                Catch ex As Exception
                End Try
                'sBU = ""
                'Try
                '    sBU = CStr(rdr("BUSINESS_UNIT_OM")).Trim.ToUpper
                'Catch ex As Exception
                'End Try
                If sBU.Length > 0 And sOrder.Length > 0 Then
                    arr.Add(New changedOrder(sBU, sOrder))
                End If
            End While
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

        m_oraCN.Close()

        Return (arr)
    End Function

    Private Sub sendEmailNotification(ByVal order As orderHdr, _
                                      ByVal ccEmails As String, _
                                      ByVal bccEmails As String, _
                                      Optional ByVal bIsShowProofLink As Boolean = True, _
                                      Optional ByVal bSuppressSendingToCustomer As Boolean = False)

        Dim msg As New System.Web.Mail.MailMessage

        msg.From = "InSiteOnline@SDI.com"
        msg.Cc = ""
        'msg.Bcc = "erwin.bautista@sdi.com;pete.doyle@sdi.com;"
        msg.Bcc = ""

        If Not (ccEmails Is Nothing) Then
            If ccEmails.Trim.Length > 0 Then
                msg.Cc = ccEmails
            End If
        End If

        If Not (bccEmails Is Nothing) Then
            If bccEmails.Trim.Length > 0 Then
                msg.Bcc = bccEmails
            End If
        End If

        'If Not (additionalBCC Is Nothing) Then
        '    For Each eAddress As String In additionalBCC
        '        If eAddress.Trim.Length > 0 Then
        '            msg.Bcc &= eAddress.Trim & ";"
        '        End If
        '    Next
        'End If

        Dim msgBody As String = ""

        msgBody &= "<center><span style='font-family:Arial;font-size:X-Large;width:256px;'>SDI Marketplace</span></center>" & vbCrLf
        msgBody &= "<center><span >In-Site&reg; Online - Order Status</span></center>"
        msgBody &= "&nbsp;" & vbCrLf

        Dim customerCareSign As String = "SDI Customer Care"
        Dim strPurchaserName As String = order.RecipientName
        Dim strPurchaserEmail As String = order.RecipientEmailAddress
        ' optional - suppress sending email to actual customer recipient
        If bSuppressSendingToCustomer Then
            strPurchaserEmail = ""
        End If
        Dim msgOrderDetail As String = ""

        ' consolidate order line(s) for email details
        '   group per line number and item Id
        Dim ordLn As orderLine = Nothing
        Dim ordLines As New ArrayList

        If order.OrderLines.Count > 0 Then
            Dim bIsLineItemFound As Boolean = False
            For Each i As orderLine In order.OrderLines
                ordLn = Nothing
                bIsLineItemFound = False
                '
                If ordLines.Count > 0 Then
                    For Each i2 As orderLine In ordLines
                        If i2.LineNo = i.LineNo And _
                           i2.ItemId = i.ItemId And _
                           i2.ItemDesc = i.ItemDesc Then
                            ordLn = i2
                            bIsLineItemFound = True
                            Exit For
                        End If
                    Next
                End If
                '
                If Not bIsLineItemFound Then
                    ' create a new and add
                    ordLn = New orderLine
                    ordLn.DemandLineNo = i.DemandLineNo
                    ordLn.ItemDesc = i.ItemDesc
                    ordLn.ItemId = i.ItemId
                    ordLn.ItemQuantity = i.ItemQuantity
                    ordLn.LineNo = i.LineNo
                    ordLn.OrderIntLineNo = i.OrderIntLineNo
                    ordLn.ReceiverId = i.ReceiverId
                    ordLn.ReceiverLineNo = i.ReceiverLineNo
                    ordLn.ShippedDate = i.ShippedDate
                    ordLn.WorkOrderNo = i.WorkOrderNo
                    ordLines.Add(ordLn)
                Else
                    ' update
                    '   add quantity to existing
                    ordLn.ItemQuantity += i.ItemQuantity
                    '   check/update ship date
                    If CDate(ordLn.ShippedDate) < CDate(i.ShippedDate) Then
                        ordLn.ShippedDate = i.ShippedDate
                        ordLn.DemandLineNo = i.DemandLineNo
                        ordLn.OrderIntLineNo = i.OrderIntLineNo
                        ordLn.WorkOrderNo = i.WorkOrderNo
                    End If
                End If
            Next
        End If

        Dim sWO As String = ""

        If ordLines.Count > 0 Then
            sWO = CType(ordLines(0), orderLine).WorkOrderNo
        End If

        Dim bldr As New System.Text.StringBuilder
        'Dim sLink As String = "<a href='" & m_scannedDocLinkURL & "'>{1}</a>"
        'Dim url As String = bldr.AppendFormat(sLink, _
        '                                      order.OrderNo, _
        '                                      "Proof of Delivery").ToString
        Dim sLink As String = ""
        Dim url As String = ""
        Dim bIsScannedShipDoc As Boolean = False
        Try
            bIsScannedShipDoc = legatoDocs.IsScannedDocExists(orderNo:=order.OrderNo, _
                                                              sqlCNstring:=m_legatoCNstring)
        Catch ex As Exception
        End Try
        If bIsScannedShipDoc Then
            ' scanned document exists in legato, so let's use that
            sLink = "<a href='" & m_scannedDocLinkURL & "'>{1}</a>"
            url = bldr.AppendFormat(sLink, _
                                    order.OrderNo, _
                                    "Proof of Delivery").ToString
        Else
            ' then we assume that this order was delivered using device
            sLink = "<a href='" & m_deviceDeliveredDocLinkURL & "'>{2}</a>"
            url = bldr.AppendFormat(sLink, _
                                    order.BusinessUnit, _
                                    order.OrderNo, _
                                    "Proof of Delivery").ToString
        End If
        bldr = Nothing

        msgOrderDetail &= "&nbsp;" & vbCrLf
        msgOrderDetail &= "<div>"
        msgOrderDetail &= "<p>Hello " & strPurchaserName & ",<br>"
        msgOrderDetail &= "&nbsp;<BR>"
        msgOrderDetail &= "Your InSiteOnline.com order number " & order.OrderNo & " has been processed and delivered/picked up.<br>"
        msgOrderDetail &= "&nbsp;<BR>"
        msgOrderDetail &= "<table id='tab2' cellSpacing='3' cellPadding='1' width='100%' border='0'>"
        msgOrderDetail &= " <tr valign='top'>"
        msgOrderDetail &= "   <td width='120'>School</td>"
        msgOrderDetail &= "   <td>" & order.ShipToLocationName & "</td>"
        msgOrderDetail &= " </tr>"
        msgOrderDetail &= " <tr valign='top'>"
        msgOrderDetail &= "   <td width='120'>Work Order</td>"
        msgOrderDetail &= "   <td>" & sWO & "</td>"
        msgOrderDetail &= " </tr>"
        If bIsShowProofLink Then
            msgOrderDetail &= " <tr valign='top'>"
            msgOrderDetail &= "   <td width='120'>&nbsp;</td>"
            msgOrderDetail &= "   <td>" & url & "</td>"
            msgOrderDetail &= " </tr>"
        End If
        msgOrderDetail &= "</table>"
        msgOrderDetail &= "&nbsp;<BR>"
        msgOrderDetail &= "Order Contents:<br>"
        msgOrderDetail &= "&nbsp;</p>"
        msgOrderDetail &= "<TABLE id='tab1' cellSpacing='1' cellPadding='1' width='100%' border='1'>" & vbCrLf
        ' headers
        msgOrderDetail &= "<TR>" & vbCrLf
        msgOrderDetail &= "  <TD align='center'>Item&nbsp;ID</TD>" & vbCrLf
        msgOrderDetail &= "  <TD align='center'>Description</TD>" & vbCrLf
        msgOrderDetail &= "  <TD align='center'>Quantity</TD>" & vbCrLf
        msgOrderDetail &= "</TR>" & vbCrLf
        ' item rows
        '   use ordLines (consolidated item lines per line# + item Id)
        If ordLines.Count > 0 Then
            For Each l As orderLine In ordLines
                msgOrderDetail &= "<TR>" & vbCrLf
                msgOrderDetail &= "  <TD align='left'>" & CStr(IIf(l.ItemId.Trim.Length = 0, "&nbsp;", l.ItemId)) & "</TD>" & vbCrLf
                msgOrderDetail &= "  <TD align='left'>" & CStr(IIf(l.ItemDesc.Trim.Length = 0, "&nbsp;", l.ItemDesc)) & "</TD>" & vbCrLf
                msgOrderDetail &= "  <TD align='right'>" & Math.Round(l.ItemQuantity, 2).ToString("###,###,###,##0.00") & "</TD>" & vbCrLf
                msgOrderDetail &= "</TR>" & vbCrLf
            Next
        Else
            msgOrderDetail &= "<TR><TD align='center' colspan='3'>We're unable to pull up item lines for this order.<br>Please contact&nbsp;" & customerCareSign & "&nbsp;for any question about this order.</TD></TR>" & vbCrLf
        End If
        msgOrderDetail &= "</TABLE>" & vbCrLf
        msgOrderDetail &= "&nbsp;<br>"
        msgOrderDetail &= "<p>Sincerely,<br>"
        msgOrderDetail &= "&nbsp;<br>"
        msgOrderDetail &= customerCareSign & "<br>"
        msgOrderDetail &= "&nbsp;<br>"
        msgOrderDetail &= "</p>"
        msgOrderDetail &= "</div>" & vbCrLf
        msgOrderDetail &= "<form id='form1'>" & vbCrLf
        ' here, user the UN-consolidated order lines for troubleshooting purposes
        If order.OrderLines.Count > 0 Then
            msgOrderDetail &= "  <input type='hidden' id='RecipientId' value='" & order.RecipientId & "' />" & vbCrLf
            msgOrderDetail &= "  <input type='hidden' id='ShipToLocation' value='" & order.ShipToLocation & "' />" & vbCrLf
            For Each l As orderLine In order.OrderLines
                msgOrderDetail &= "  <input type='hidden' id='LineNo' value='" & l.LineNo.ToString & "' />" & vbCrLf
                msgOrderDetail &= "  <input type='hidden' id='OrderIntLineNo' value='" & l.OrderIntLineNo.ToString & "' />" & vbCrLf
                msgOrderDetail &= "  <input type='hidden' id='ItemId' value='" & l.ItemId & "' />" & vbCrLf
                msgOrderDetail &= "  <input type='hidden' id='ItemDesc' value='" & l.ItemDesc & "' />" & vbCrLf
                msgOrderDetail &= "  <input type='hidden' id='ItemQuantity' value='" & l.ItemQuantity.ToString & "' />" & vbCrLf
                msgOrderDetail &= "  <input type='hidden' id='ReceiverId' value='" & l.ReceiverId & "' />" & vbCrLf
                msgOrderDetail &= "  <input type='hidden' id='ReceiverLineNo' value='" & l.ReceiverLineNo.ToString & "' />" & vbCrLf
                msgOrderDetail &= "  <input type='hidden' id='ShippedDate' value='" & l.ShippedDate & "' />" & vbCrLf
                msgOrderDetail &= "  <input type='hidden' id='WorkOrderNo' value='" & l.WorkOrderNo & "' />" & vbCrLf
            Next
        End If
        msgOrderDetail &= "</form>" & vbCrLf

        msg.Subject = "In-Site® Online - Delivery Notification of Order Number " & order.OrderNo & ""
        msg.Body = msgBody & msgOrderDetail
        msg.BodyFormat = System.Web.Mail.MailFormat.Html

        'If m_oraCN.DataSource.ToUpper.IndexOf("RPTG") > -1 Or _
        '   m_oraCN.DataSource.ToUpper.IndexOf("DEVL") > -1 Or _
        '   m_oraCN.DataSource.ToUpper.IndexOf("PLGR") > -1 Then
        '    msg.To = "DoNotSendPLGR@sdi.com"
        '    msg.Subject &= " (test run)"
        'Else
        '    msg.To = strPurchaserEmail
        'End If
        msg.To = strPurchaserEmail

        UpdEmailOut.UpdEmailOut.UpdEmailOut(msg.Subject, _
                                            msg.From, _
                                            msg.To, _
                                            msg.Cc, _
                                            msg.Bcc, _
                                            "N", _
                                            msg.Body, _
                                            m_oraCN)

        ordLines = Nothing

    End Sub

    Private Sub flagOrderAsProcessed(ByVal order As orderHdr)

        '"SET EMAIL_DATETIME = TO_DATE('" & Now.ToString("MM/dd/yyyy HH:mm:ss") & "', 'MM/DD/YYYY HH24:MI:SS') " & vbCrLf & _
        '"WHERE BUSINESS_UNIT_OM = '" & order.BusinessUnit & "' " & vbCrLf & _

        Dim cmd As OleDbCommand = Nothing

        For Each l As orderLine In order.OrderLines

            Dim sqlAdd As String = "" & _
                                   "INSERT INTO PS_ISA_ORDSTAT_EML " & vbCrLf & _
                                   "(" & vbCrLf & _
                                   " BUSINESS_UNIT_OM" & vbCrLf & _
                                   ",ORDER_NO " & vbCrLf & _
                                   ",LINE_NBR " & vbCrLf & _
                                   ",ORDER_INT_LINE_NO " & vbCrLf & _
                                   ",DEMAND_LINE_NO " & vbCrLf & _
                                   ",RECEIVER_ID " & vbCrLf & _
                                   ",RECV_LN_NBR " & vbCrLf & _
                                   ",EMPLID " & vbCrLf & _
                                   ",ISA_ORDER_STATUS " & vbCrLf & _
                                   ",EMAIL_DATETIME " & vbCrLf & _
                                   ")" & vbCrLf & _
                                   "VALUES " & vbCrLf & _
                                   "(" & vbCrLf & _
                                   " '" & order.BusinessUnit & "' " & vbCrLf & _
                                   ",'" & order.OrderNo & "' " & vbCrLf & _
                                   ",'" & l.LineNo.ToString & "' " & vbCrLf & _
                                   ",'" & l.OrderIntLineNo.ToString & "' " & vbCrLf & _
                                   ",'" & l.DemandLineNo.ToString & "' " & vbCrLf & _
                                   ",' ' " & vbCrLf & _
                                   ",'0' " & vbCrLf & _
                                   ",'" & order.RecipientId & "' " & vbCrLf & _
                                   ",'7' " & vbCrLf & _
                                   ",SYSDATE " & vbCrLf & _
                                   ")" & vbCrLf & _
                                   ""
            If (m_oraCN.State <> ConnectionState.Closed) Then
                m_oraCN.Close()
            End If
            m_oraCN.Open()

            cmd = m_oraCN.CreateCommand
            cmd.CommandText = sqlAdd
            cmd.CommandType = CommandType.Text
            Try
                Dim i As Integer = cmd.ExecuteNonQuery
            Catch ex As Exception
            End Try
            Try
                cmd.Dispose()
            Catch ex As Exception
            Finally
                cmd = Nothing
            End Try

            m_oraCN.Close()

        Next

    End Sub

    Private Function getShippedNSTKOrders(ByVal bu As String, _
                                          ByVal dtStart As DateTime, _
                                          ByVal dtEnd As DateTime) As ArrayList
        Const ordStatusLog_SHIPPED As String = "6"
        Dim arr As New ArrayList

        If m_oraCN.State <> ConnectionState.Closed Then
            m_oraCN.Close()
        End If
        m_oraCN.Open()

        Dim sql As String = "" & vbCrLf & _
                            "SELECT " & vbCrLf & _
                            " A.ORDER_NO " & vbCrLf & _
                            ",A.BUSINESS_UNIT_OM " & vbCrLf & _
                            "FROM PS_ISAORDSTATUSLOG A" & vbCrLf & _
                            "WHERE (A.DTTM_STAMP BETWEEN TO_DATE('" & dtStart.ToString("MM/dd/yyyy HH:mm:ss") & "','MM/DD/YYYY HH24:MI:SS') AND TO_DATE('" & dtEnd.ToString("MM/dd/yyyy HH:mm:ss") & "','MM/DD/YYYY HH24:MI:SS')) " & vbCrLf & _
                            "  AND A.BUSINESS_UNIT_OM = '" & bu & "' " & vbCrLf & _
                            "  AND A.ISA_ORDER_STATUS = '" & ordStatusLog_SHIPPED & "' " & vbCrLf & _
                            "  AND NOT EXISTS (" & vbCrLf & _
                            "                  SELECT 'X'" & vbCrLf & _
                            "  		           FROM PS_ORD_HEADER B" & vbCrLf & _
                            "                  WHERE B.BUSINESS_UNIT = A.BUSINESS_UNIT_OM" & vbCrLf & _
                            "                    AND B.ORDER_NO = A.ORDER_NO" & vbCrLf & _
                            "                 )" & vbCrLf & _
                            "  AND NOT EXISTS (" & vbCrLf & _
                            "                  SELECT 'X'" & vbCrLf & _
                            "                  FROM PS_PO_LINE_DISTRIB C" & vbCrLf & _
                            "                  WHERE C.BUSINESS_UNIT = 'ISA00'" & vbCrLf & _
                            "                    AND C.REQ_ID = A.ORDER_NO" & vbCrLf & _
                            "                    AND C.DISTRIB_LN_STATUS <> 'X'" & vbCrLf & _
                            "                    AND C.QTY_PO > (" & vbCrLf & _
                            "                                    SELECT SUM(NVL(E.QTY_LN_ACCPT_VUOM,0)) AS QTY_LN_ACCPT_VUOM" & vbCrLf & _
                            "                                    FROM PS_RECV_LN E" & vbCrLf & _
                            "                                    WHERE C.BUSINESS_UNIT = E.BUSINESS_UNIT_PO" & vbCrLf & _
                            "                                      AND C.PO_ID         = E.PO_ID" & vbCrLf & _
                            "                                      AND C.LINE_NBR      = E.LINE_NBR" & vbCrLf & _
                            "                                   )" & vbCrLf & _
                            "                 )" & vbCrLf & _
                            "GROUP BY A.ORDER_NO, A.BUSINESS_UNIT_OM " & vbCrLf & _
                            ""
        Dim cmd As OleDbCommand = m_oraCN.CreateCommand
        cmd.CommandText = sql
        cmd.CommandType = CommandType.Text

        Dim rdr As OleDbDataReader = Nothing

        Try
            rdr = cmd.ExecuteReader
        Catch ex As Exception
        End Try

        If Not (rdr Is Nothing) Then
            Dim sBU As String = ""
            Dim sOrder As String = ""
            While rdr.Read
                sBU = ""
                Try
                    sBU = CStr(rdr("BUSINESS_UNIT_OM")).Trim.ToUpper
                Catch ex As Exception
                End Try
                sOrder = ""
                Try
                    sOrder = CStr(rdr("ORDER_NO")).Trim.ToUpper
                Catch ex As Exception
                End Try
                If sBU.Length > 0 And sOrder.Length > 0 Then
                    arr.Add(New changedOrder(sBU, sOrder))
                End If
            End While
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

        m_oraCN.Close()

        Return (arr)
    End Function

End Class
