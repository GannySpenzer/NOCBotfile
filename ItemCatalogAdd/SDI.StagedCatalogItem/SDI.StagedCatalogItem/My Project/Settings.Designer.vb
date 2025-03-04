﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.18444
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On


Namespace My
    
    <Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0"),  _
     Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
    Partial Friend NotInheritable Class MySettings
        Inherits Global.System.Configuration.ApplicationSettingsBase
        
        Private Shared defaultInstance As MySettings = CType(Global.System.Configuration.ApplicationSettingsBase.Synchronized(New MySettings()),MySettings)
        
#Region "My.Settings Auto-Save Functionality"
#If _MyType = "WindowsForms" Then
    Private Shared addedHandler As Boolean

    Private Shared addedHandlerLockObject As New Object

    <Global.System.Diagnostics.DebuggerNonUserCodeAttribute(), Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)> _
    Private Shared Sub AutoSaveSettings(ByVal sender As Global.System.Object, ByVal e As Global.System.EventArgs)
        If My.Application.SaveMySettingsOnExit Then
            My.Settings.Save()
        End If
    End Sub
#End If
#End Region
        
        Public Shared ReadOnly Property [Default]() As MySettings
            Get
                
#If _MyType = "WindowsForms" Then
               If Not addedHandler Then
                    SyncLock addedHandlerLockObject
                        If Not addedHandler Then
                            AddHandler My.Application.Shutdown, AddressOf AutoSaveSettings
                            addedHandler = True
                        End If
                    End SyncLock
                End If
#End If
                Return defaultInstance
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("Provider=OraOLEDB.Oracle;Data Source=PROD;User Id=EINTERNET;Password=einternet;")>  _
        Public ReadOnly Property oraCNString() As String
            Get
                Return CType(Me("oraCNString"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("server=SQL2012;uid=ItemAddWorker;pwd=sdiadmin;initial catalog={0};")>  _
        Public ReadOnly Property sql2012CNString() As String
            Get
                Return CType(Me("sql2012CNString"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("server=DAZZLE;uid=sa;pwd=sdiadmin;initial catalog={0};")>  _
        Public ReadOnly Property sqlDAZZLECNString() As String
            Get
                Return CType(Me("sqlDAZZLECNString"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("server=SDICLOUD;uid=spAdmin;pwd=Sharepoint2013;initial catalog={0};")>  _
        Public ReadOnly Property sqlSDICLOUDCNString() As String
            Get
                Return CType(Me("sqlSDICLOUDCNString"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("LatestRunInfo.xml")>  _
        Public ReadOnly Property LastRunInfoXML() As String
            Get
                Return CType(Me("LastRunInfoXML"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("GetNewModPSItem.sql")>  _
        Public ReadOnly Property qryGetNewModPSItem() As String
            Get
                Return CType(Me("qryGetNewModPSItem"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("checkCustomerItemInCatalog.sql")>  _
        Public ReadOnly Property qryCheckCustomerItemInCatalog() As String
            Get
                Return CType(Me("qryCheckCustomerItemInCatalog"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("server=192.168.253.52;uid=sa;pwd=coca-cola;initial catalog=Contentplus;")>  _
        Public ReadOnly Property sqlCPlusCNString() As String
            Get
                Return CType(Me("sqlCPlusCNString"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("GetCatalogProductView.sql")>  _
        Public ReadOnly Property qryGetCatalogProductViewList() As String
            Get
                Return CType(Me("qryGetCatalogProductViewList"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("GetSiteProductView.sql")>  _
        Public ReadOnly Property qryGetSiteProductViewList() As String
            Get
                Return CType(Me("qryGetSiteProductViewList"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("GetSiteItemPrefix.sql")>  _
        Public ReadOnly Property qryGetSiteItemPrefixList() As String
            Get
                Return CType(Me("qryGetSiteItemPrefixList"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("InsertCP_JUNCTION.sql")>  _
        Public ReadOnly Property qryInsertCP_JUNCTION() As String
            Get
                Return CType(Me("qryInsertCP_JUNCTION"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("UpdateCP_JUNCTION.sql")>  _
        Public ReadOnly Property qryUpdateCP_JUNCTION() As String
            Get
                Return CType(Me("qryUpdateCP_JUNCTION"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("StageClassAvailableProducts_Insert.sql")>  _
        Public ReadOnly Property qryStageClassAvailProdInsert() As String
            Get
                Return CType(Me("qryStageClassAvailProdInsert"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("StageClassAvailableProducts_Update.sql")>  _
        Public ReadOnly Property qryStageClassAvailProdUpdate() As String
            Get
                Return CType(Me("qryStageClassAvailProdUpdate"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("RemoveItemFromClassAvailableProducts.sql")>  _
        Public ReadOnly Property qryRemoveOriginClassAvailProd() As String
            Get
                Return CType(Me("qryRemoveOriginClassAvailProd"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("MainClassAvailableProducts_Update.sql")>  _
        Public ReadOnly Property qryMainClassAvailProdUpdate() As String
            Get
                Return CType(Me("qryMainClassAvailProdUpdate"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("MainClassAvailableProducts_Insert.sql")>  _
        Public ReadOnly Property qryMainClassAvailProdInsert() As String
            Get
                Return CType(Me("qryMainClassAvailProdInsert"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("UpdateScottsdaleItemTableMfgPartNumber.sql")>  _
        Public ReadOnly Property qryUpdateScottsdaleItemMfgPartNo() As String
            Get
                Return CType(Me("qryUpdateScottsdaleItemMfgPartNo"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("GetItemClassId.sql")>  _
        Public ReadOnly Property qryGetItemClassId() As String
            Get
                Return CType(Me("qryGetItemClassId"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("StageClassAvailableProducts_UpdateFlag.sql")>  _
        Public ReadOnly Property qryStageClassAvailProdUpdateFlag() As String
            Get
                Return CType(Me("qryStageClassAvailProdUpdateFlag"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("GetItemClassId_Staged.sql")>  _
        Public ReadOnly Property qryGetItemClassIdStaged() As String
            Get
                Return CType(Me("qryGetItemClassIdStaged"),String)
            End Get
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("UpdateCPJunctionViaPartNumberIfNotEqual.sql")>  _
        Public ReadOnly Property qryUpdateCPJunctionViaPartNoNonEqual() As String
            Get
                Return CType(Me("qryUpdateCPJunctionViaPartNoNonEqual"),String)
            End Get
        End Property
    End Class
End Namespace

Namespace My
    
    <Global.Microsoft.VisualBasic.HideModuleNameAttribute(),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>  _
    Friend Module MySettingsProperty
        
        <Global.System.ComponentModel.Design.HelpKeywordAttribute("My.Settings")>  _
        Friend ReadOnly Property Settings() As Global.SDI.StagedCatalogItem.My.MySettings
            Get
                Return Global.SDI.StagedCatalogItem.My.MySettings.Default
            End Get
        End Property
    End Module
End Namespace
