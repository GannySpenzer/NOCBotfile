Imports System
Imports System.Xml


Public Class punchOutGroupIdentifier

    Implements IDisposable

    Private m_punchOutSiteId As String = ""
    Private m_punchOutSiteName As String = ""
    Private m_grps As Hashtable = Nothing
    Private m_punchOutGrpDefinitionFile As String = ""


    Public Sub New(ByVal punchOutSiteId As String)
        m_punchOutSiteId = punchOutSiteId
    End Sub

    Public ReadOnly Property [Id]() As String
        Get
            Return m_punchOutSiteId
        End Get
    End Property

    Public ReadOnly Property GroupDefinitionFile() As String
        Get
            Return m_punchOutGrpDefinitionFile
        End Get
    End Property

    Public Property [Name]() As String
        Get
            Return m_punchOutSiteName
        End Get
        Set(ByVal Value As String)
            m_punchOutSiteName = Value
        End Set
    End Property

    '// "key" part should be the "group id" (eg, kelloggs)
    '// "value" will be of type "punchOutGrpSites"
    Public ReadOnly Property Groups() As Hashtable
        Get
            If (m_grps Is Nothing) Then
                m_grps = New Hashtable
            End If
            Return m_grps
        End Get
    End Property

    Public Shared Function LoadPunchOutGroupIdentifier(ByVal punchOutSiteId As String, _
                                                       ByVal punchOutGrpDefinitionFile As String) As punchOutGroupIdentifier
        Dim rtn As String = "punchOutGroupIdentifier.LoadPunchOutGroupIdentifier"
        Dim grpIdentifier As punchOutGroupIdentifier = Nothing

        punchOutSiteId = punchOutSiteId.Trim.ToUpper
        punchOutGrpDefinitionFile = punchOutGrpDefinitionFile.Trim

        If punchOutSiteId.Length > 0 Then
            Try
                grpIdentifier = New punchOutGroupIdentifier(punchOutSiteId)

                Dim cfg As New XmlDocument
                Dim stringer As theStringer = New theStringer(Common.LoadPathFile(punchOutGrpDefinitionFile))
                cfg.LoadXml(Xml:=stringer.ToString)
                stringer = Nothing

                Dim grpSite As punchOutGrpSites = Nothing
                Dim sGrpId As String = ""
                Dim sGrpName As String = ""

                Dim site As punchOutGrpSite = Nothing
                Dim siteId As String = ""
                Dim siteName As String = ""
                Dim siteCustId As String = ""

                Dim nodeGrps As XmlNode = cfg.SelectSingleNode(XPath:="Groups")

                If Not (nodeGrps Is Nothing) Then
                    If nodeGrps.ChildNodes.Count > 0 Then
                        For Each nodeGrp As XmlNode In nodeGrps.ChildNodes
                            If nodeGrp.NodeType = XmlNodeType.Element And nodeGrp.Name = "Group" Then
                                '// group
                                grpSite = Nothing
                                sGrpId = ""
                                sGrpName = ""
                                Try
                                    sGrpId = nodeGrp.Attributes(Name:="id").Value
                                Catch ex As Exception
                                End Try
                                Try
                                    sGrpName = nodeGrp.Attributes(Name:="desc").Value
                                Catch ex As Exception
                                End Try
                                If sGrpId.Trim.Length > 0 Then
                                    Try
                                        grpSite = DirectCast(grpIdentifier.Groups.Item(sGrpId.Trim), punchOutGrpSites)
                                    Catch ex As Exception
                                    End Try
                                End If
                                If (grpSite Is Nothing) Then
                                    grpSite = New punchOutGrpSites(sGrpId)
                                    grpSite.Name = sGrpName
                                    grpIdentifier.Groups.Add(grpSite.Id, grpSite)
                                End If
                                '// load individual site for this group
                                For Each nodeSite As XmlNode In nodeGrp.ChildNodes
                                    siteId = ""
                                    siteName = ""
                                    siteCustId = ""
                                    Try
                                        siteId = nodeSite.Attributes(Name:="id").Value
                                    Catch ex As Exception
                                    End Try
                                    Try
                                        siteName = nodeSite.Attributes(Name:="desc").Value
                                    Catch ex As Exception
                                    End Try
                                    Try
                                        siteCustId = nodeSite.Attributes(Name:="custId").Value
                                    Catch ex As Exception
                                    End Try
                                    If siteId.Trim.Length > 0 Then
                                        If grpSite.easySiteIdSearch.IndexOf(siteId) = -1 Then
                                            site = New punchOutGrpSite(siteId)
                                            site.Name = siteName
                                            site.CustomerId = siteCustId
                                            grpSite.Sites.Add(site)
                                        End If
                                    End If
                                Next
                            End If      ' If nodeGrp.NodeType = XmlNodeType.Element And nodeGrp.Name = "Group"
                        Next
                    End If
                End If

                cfg = Nothing
            Catch ex As Exception
                Throw New ApplicationException(rtn & "::" & ex.Message, ex)
            End Try
        End If

        Return grpIdentifier
    End Function

#Region " IDisposable Implementations "

    Private m_bIsDisposing As Boolean = False

    Public Sub Dispose() Implements System.IDisposable.Dispose
        If Not m_bIsDisposing Then
            m_bIsDisposing = True
            'Try
            '    m_identityFrom.Dispose()
            'Catch ex As Exception
            'Finally
            '    m_identityFrom = Nothing
            'End Try
            'Try
            '    m_identityTo.Dispose()
            'Catch ex As Exception
            'Finally
            '    m_identityTo = Nothing
            'End Try
            'Try
            '    m_identitySender.Dispose()
            'Catch ex As Exception
            'Finally
            '    m_identitySender = Nothing
            'End Try
        End If
    End Sub

#End Region

End Class
