<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProductVersion>
    </ProductVersion>
    <SchemaVersion>
    </SchemaVersion>
    <ProjectGuid>{E072C679-8592-4690-B914-A7A1AFFD704B}</ProjectGuid>
    <ProjectTypeGuids>{349c5851-65df-11da-9384-00065b846f21};{F184B08F-C81C-45F6-A57F-5ABD9991F28F}</ProjectTypeGuids>
    <OutputType>Library</OutputType>
    <RootNamespace>SDI.Walmart.API</RootNamespace>
    <AssemblyName>SDI.Walmart.API</AssemblyName>
    <TargetFrameworkVersion>v4.7.1</TargetFrameworkVersion>
    <MyType>Custom</MyType>
    <UseIISExpress>true</UseIISExpress>
    <Use64BitIISExpress />
    <IISExpressSSLPort />
    <IISExpressAnonymousAuthentication />
    <IISExpressWindowsAuthentication />
    <IISExpressUseClassicPipelineMode />
    <UseGlobalApplicationHostFile />
    <NuGetPackageImportStamp>
    </NuGetPackageImportStamp>
    <SccProjectName>SAK</SccProjectName>
    <SccLocalPath>SAK</SccLocalPath>
    <SccAuxPath>SAK</SccAuxPath>
    <SccProvider>SAK</SccProvider>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <DefineDebug>true</DefineDebug>
    <DefineTrace>true</DefineTrace>
    <OutputPath>bin\</OutputPath>
    <DocumentationFile>SDI.Walmart.API.xml</DocumentationFile>
    <NoWarn>42016,41999,42017,42018,42019,42032,42036,42020,42021,42022</NoWarn>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>pdbonly</DebugType>
    <DefineDebug>false</DefineDebug>
    <DefineTrace>true</DefineTrace>
    <Optimize>true</Optimize>
    <OutputPath>bin\</OutputPath>
    <DocumentationFile>SDI.Walmart.API.xml</DocumentationFile>
    <NoWarn>42016,41999,42017,42018,42019,42032,42036,42020,42021,42022</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="ClosedXML, Version=0.95.3.0, Culture=neutral, processorArchitecture=MSIL">
      <HintPath>..\packages\ClosedXML.0.95.3\lib\net46\ClosedXML.dll</HintPath>
    </Reference>
    <Reference Include="CrystalDecisions.CrystalReports.Engine, Version=13.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304, processorArchitecture=MSIL">
      <SpecificVersion>False</SpecificVersion>
      <HintPath>ReferenceDll\CrystalDecisions.CrystalReports.Engine.dll</HintPath>
    </Reference>
    <Reference Include="CrystalDecisions.Shared, Version=13.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304, processorArchitecture=MSIL">
      <SpecificVersion>False</SpecificVersion>
      <HintPath>ReferenceDll\CrystalDecisions.Shared.dll</HintPath>
    </Reference>
    <Reference Include="DocumentFormat.OpenXml, Version=2.7.2.0, Culture=neutral, PublicKeyToken=8fb06cb64d019a17, processorArchitecture=MSIL">
      <HintPath>..\packages\DocumentFormat.OpenXml.2.7.2\lib\net46\DocumentFormat.OpenXml.dll</HintPath>
    </Reference>
    <Reference Include="ExcelNumberFormat, Version=1.0.10.0, Culture=neutral, PublicKeyToken=23c6f5d73be07eca, processorArchitecture=MSIL">
      <HintPath>..\packages\ExcelNumberFormat.1.0.10\lib\net20\ExcelNumberFormat.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.CodeDom.Providers.DotNetCompilerPlatform, Version=3.6.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.3.6.0\lib\net45\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.CSharp" />
    <Reference Include="Microsoft.Owin, Version=4.1.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.Owin.4.1.0\lib\net45\Microsoft.Owin.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.Owin.Cors, Version=4.1.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.Owin.Cors.4.1.0\lib\net45\Microsoft.Owin.Cors.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.Owin.Host.SystemWeb, Version=4.1.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.Owin.Host.SystemWeb.4.1.0\lib\net45\Microsoft.Owin.Host.SystemWeb.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.Owin.Security, Version=4.1.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.Owin.Security.4.1.0\lib\net45\Microsoft.Owin.Security.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.Owin.Security.OAuth, Version=4.1.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.Owin.Security.OAuth.4.1.0\lib\net45\Microsoft.Owin.Security.OAuth.dll</HintPath>
    </Reference>
    <Reference Include="Newtonsoft.Json, Version=12.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed, processorArchitecture=MSIL">
      <HintPath>..\packages\Newtonsoft.Json.12.0.3\lib\net45\Newtonsoft.Json.dll</HintPath>
      <Private>True</Private>
    </Reference>
    <Reference Include="Oracle.DataAccess, Version=2.112.1.0, Culture=neutral, PublicKeyToken=89b483f429c47342, processorArchitecture=x86">
      <HintPath>..\packages\Oracle.DataAccess.x86.2.112.1.0\lib\Oracle.DataAccess.dll</HintPath>
    </Reference>
    <Reference Include="Oracle.ManagedDataAccess, Version=4.122.19.1, Culture=neutral, PublicKeyToken=89b483f429c47342, processorArchitecture=MSIL">
      <HintPath>..\packages\Oracle.ManagedDataAccess.19.8.0\lib\net40\Oracle.ManagedDataAccess.dll</HintPath>
    </Reference>
    <Reference Include="Owin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=f0ebd12fd5e55cc5, processorArchitecture=MSIL">
      <HintPath>..\packages\Owin.1.0\lib\net40\Owin.dll</HintPath>
    </Reference>
    <Reference Include="SDI.PunchOut, Version=4.0.1.3, Culture=neutral, processorArchitecture=MSIL">
      <SpecificVersion>False</SpecificVersion>
      <HintPath>ReferenceDll\SDI.PunchOut.dll</HintPath>
    </Reference>
    <Reference Include="System" />
    <Reference Include="System.Data" />
    <Reference Include="System.Drawing" />
    <Reference Include="System.Core" />
    <Reference Include="System.Data.DataSetExtensions" />
    <Reference Include="System.IO.FileSystem.Primitives, Version=4.0.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL">
      <HintPath>..\packages\System.IO.FileSystem.Primitives.4.0.1\lib\net46\System.IO.FileSystem.Primitives.dll</HintPath>
      <Private>True</Private>
      <Private>True</Private>
    </Reference>
    <Reference Include="System.IO.Packaging, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL">
      <HintPath>..\packages\System.IO.Packaging.4.0.0\lib\net46\System.IO.Packaging.dll</HintPath>
    </Reference>
    <Reference Include="System.Net.Http" />
    <Reference Include="System.Net.Http.Formatting, Version=5.2.7.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.AspNet.WebApi.Client.5.2.7\lib\net45\System.Net.Http.Formatting.dll</HintPath>
    </Reference>
    <Reference Include="System.Runtime.Serialization" />
    <Reference Include="System.ServiceModel" />
    <Reference Include="System.Web.Cors, Version=5.2.7.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.AspNet.Cors.5.2.7\lib\net45\System.Web.Cors.dll</HintPath>
    </Reference>
    <Reference Include="System.Web.Extensions" />
    <Reference Include="System.Web.Http, Version=5.2.7.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.AspNet.WebApi.Core.5.2.7\lib\net45\System.Web.Http.dll</HintPath>
    </Reference>
    <Reference Include="System.Web.Http.Cors, Version=5.2.7.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
      <HintPath>..\packages\Microsoft.AspNet.WebApi.Cors.5.2.7\lib\net45\System.Web.Http.Cors.dll</HintPath>
    </Reference>
    <Reference Include="System.Xml.Linq" />
    <Reference Include="System.Web.DynamicData" />
    <Reference Include="System.Web.Entity" />
    <Reference Include="System.Web.ApplicationServices" />
    <Reference Include="System.ComponentModel.DataAnnotations" />
    <Reference Include="System.Web" />
    <Reference Include="System.Xml" />
    <Reference Include="System.Configuration" />
    <Reference Include="System.Web.Services" />
    <Reference Include="System.EnterpriseServices" />
    <Reference Include="UpdEmailOut, Version=1.7.7047.29610, Culture=neutral, processorArchitecture=MSIL">
      <SpecificVersion>False</SpecificVersion>
      <HintPath>ReferenceDll\UpdEmailOut.dll</HintPath>
    </Reference>
    <Reference Include="WindowsBase" />
  </ItemGroup>
  <ItemGroup>
    <Reference Include="System.Web.Razor">
      <HintPath>..\packages\Microsoft.AspNet.Razor.3.2.4\lib\net45\System.Web.Razor.dll</HintPath>
    </Reference>
    <Reference Include="System.Web.Webpages">
      <HintPath>..\packages\Microsoft.AspNet.Webpages.3.2.4\lib\net45\System.Web.Webpages.dll</HintPath>
    </Reference>
    <Reference Include="System.Web.Webpages.Deployment">
      <HintPath>..\packages\Microsoft.AspNet.Webpages.3.2.4\lib\net45\System.Web.Webpages.Deployment.dll</HintPath>
    </Reference>
    <Reference Include="System.Web.Webpages.Razor">
      <HintPath>..\packages\Microsoft.AspNet.Webpages.3.2.4\lib\net45\System.Web.Webpages.Razor.dll</HintPath>
    </Reference>
    <Reference Include="System.Web.Helpers">
      <HintPath>..\packages\Microsoft.AspNet.Webpages.3.2.4\lib\net45\System.Web.Helpers.dll</HintPath>
    </Reference>
    <Reference Include="Microsoft.Web.Infrastructure">
      <HintPath>..\packages\Microsoft.Web.Infrastructure.1.0.0.0\lib\net40\Microsoft.Web.Infrastructure.dll</HintPath>
    </Reference>
    <Reference Include="System.Web.Mvc">
      <HintPath>..\packages\Microsoft.AspNet.Mvc.5.2.4\lib\net45\System.Web.Mvc.dll</HintPath>
    </Reference>
    <Reference Include="System.Web.Http.WebHost">
      <HintPath>..\packages\Microsoft.AspNet.WebApi.WebHost.5.2.4\lib\net45\System.Web.Http.WebHost.dll</HintPath>
    </Reference>
  </ItemGroup>
  <ItemGroup>
    <Import Include="Microsoft.VisualBasic" />
    <Import Include="System" />
    <Import Include="System.Collections" />
    <Import Include="System.Collections.Generic" />
    <Import Include="System.Data" />
    <Import Include="System.Linq" />
    <Import Include="System.Xml.Linq" />
    <Import Include="System.Diagnostics" />
    <Import Include="System.Collections.Specialized" />
    <Import Include="System.Configuration" />
    <Import Include="System.Text" />
    <Import Include="System.Text.RegularExpressions" />
    <Import Include="System.Web" />
    <Import Include="System.Web.Caching" />
    <Import Include="System.Web.SessionState" />
    <Import Include="System.Web.Security" />
    <Import Include="System.Web.Profile" />
    <Import Include="System.Web.UI" />
    <Import Include="System.Web.UI.WebControls" />
    <Import Include="System.Web.UI.WebControls.WebParts" />
    <Import Include="System.Web.UI.HtmlControls" />
  </ItemGroup>
  <ItemGroup>
    <Content Include="configSetting.xml" />
    <None Include="Connected Services\sdiFileUploader\fileUploader.disco" />
    <None Include="Connected Services\sdiFileUploader\configuration91.svcinfo" />
    <None Include="Connected Services\sdiFileUploader\configuration.svcinfo" />
    <None Include="Connected Services\sdiFileUploader\Reference.svcmap">
      <Generator>WCF Proxy Generator</Generator>
      <LastGenOutput>Reference.vb</LastGenOutput>
    </None>
    <Content Include="Global.asax" />
    <Content Include="Images\001.jpg" />
    <Content Include="Images\120px-Go-jump_svg.jpg" />
    <Content Include="Images\1x1.gif" />
    <Content Include="Images\add_to_cart.png" />
    <Content Include="Images\amazon.jpg" />
    <Content Include="Images\Appliedlogo01.gif" />
    <Content Include="Images\approved.png" />
    <Content Include="Images\ApproveQuotes.png" />
    <Content Include="Images\archive.png" />
    <Content Include="Images\archive_white.png" />
    <Content Include="Images\arrow_left.gif" />
    <Content Include="Images\arrow_left_sdi.gif" />
    <Content Include="Images\arrow_right.gif" />
    <Content Include="Images\arrow_right_blue.gif" />
    <Content Include="Images\arrow_right_sav.gif" />
    <Content Include="Images\arrow_right_white.gif" />
    <Content Include="Images\Ascendlogo.jpg" />
    <Content Include="Images\aWpU8J.gif" />
    <Content Include="Images\Axiall-lrg-panel.png" />
    <Content Include="Images\back-icon.jpg" />
    <Content Include="Images\back.gif" />
    <Content Include="Images\banner-small.png" />
    <Content Include="Images\bg_control_nav.png" />
    <Content Include="Images\blue-right-arrow.gif" />
    <Content Include="Images\Border States.png" />
    <Content Include="Images\btnNext_hover.png" />
    <Content Include="Images\btnNext_normal.png" />
    <Content Include="Images\btnPrev_hover.png" />
    <Content Include="Images\btnPrev_normal.png" />
    <Content Include="Images\btn_add_to_cart.gif" />
    <Content Include="Images\bullet.gif" />
    <Content Include="Images\cal.gif" />
    <Content Include="Images\careers.gif" />
    <Content Include="Images\careers_on.gif" />
    <Content Include="Images\carousel-left.png" />
    <Content Include="Images\carousel-right.png" />
    <Content Include="Images\cart.gif" />
    <Content Include="Images\cbar_top_border.png" />
    <Content Include="Images\cc-aex.jpg" />
    <Content Include="Images\cc-aex.png" />
    <Content Include="Images\cc-dc.png" />
    <Content Include="Images\cc-discover.png" />
    <Content Include="Images\cc-jcb.jpg" />
    <Content Include="Images\cc-jcb.png" />
    <Content Include="Images\cc-mc.png" />
    <Content Include="Images\cc-visa.jpg" />
    <Content Include="Images\cc-visa.png" />
    <Content Include="Images\chart-icon.png" />
    <Content Include="Images\CheckCatalog.gif" />
    <Content Include="Images\check_green.gif" />
    <Content Include="Images\check_red.gif" />
    <Content Include="Images\childsafe.jpg" />
    <Content Include="Images\clear.gif" />
    <Content Include="Images\ClientImages\EMCOR-LOGO.png" />
    <Content Include="Images\close-hover.png" />
    <Content Include="Images\Close.GIF" />
    <Content Include="Images\close.png" />
    <Content Include="Images\company.gif" />
    <Content Include="Images\company_on.gif" />
    <Content Include="Images\contact_us.gif" />
    <Content Include="Images\contact_us_on.gif" />
    <Content Include="Images\Core6-Logo.png" />
    <Content Include="Images\core_values.png" />
    <Content Include="Images\cross_ref_motif.gif" />
    <Content Include="Images\delete_icon.png" />
    <Content Include="Images\DIAGEO.jpg" />
    <Content Include="Images\diapo\next.png" />
    <Content Include="Images\diapo\pause.png" />
    <Content Include="Images\diapo\play.png" />
    <Content Include="Images\diapo\prev.png" />
    <Content Include="Images\digikey.png" />
    <Content Include="Images\dot.gif" />
    <Content Include="Images\dot_vert.gif" />
    <Content Include="Images\down_arrow.gif" />
    <Content Include="Images\down_arrow2.gif" />
    <Content Include="Images\ecologo.jpg" />
    <Content Include="Images\EXPLORER.BMP" />
    <Content Include="Images\facilities_hp_on.gif" />
    <Content Include="Images\facilities_maintenance.gif" />
    <Content Include="Images\facilities_maintenance_on.gif" />
    <Content Include="Images\facilities_main_hp.gif" />
    <Content Include="Images\FastenalLogo.PNG" />
    <Content Include="Images\favicon.ico" />
    <Content Include="Images\favicon.png" />
    <Content Include="Images\favorites.gif" />
    <Content Include="Images\favotite-icon.png" />
    <Content Include="Images\Ferguson_logo.jpg" />
    <Content Include="Images\filter.png" />
    <Content Include="Images\finish.gif" />
    <Content Include="Images\fishersci-new.jpg" />
    <Content Include="Images\gallery-cart-cancel.png" />
    <Content Include="Images\gears_ani_0.gif" />
    <Content Include="Images\gears_ani_1.gif" />
    <Content Include="Images\gears_ani_2.gif" />
    <Content Include="Images\gears_ani_3.gif" />
    <Content Include="Images\gears_an_original.gif" />
    <Content Include="Images\GHX_logo.bmp" />
    <Content Include="Images\global_reach_hp.gif" />
    <Content Include="Images\global_reach_on.gif" />
    <Content Include="Images\Grainger.logo.bmp" />
    <Content Include="Images\grainger.logo.gif" />
    <Content Include="Images\GraybR.logo.bmp" />
    <Content Include="Images\green arrow.jpg" />
    <Content Include="Images\greenseal.bmp" />
    <Content Include="Images\greenseal3.jpg" />
    <Content Include="Images\header_enter.gif" />
    <Content Include="Images\header_Login.gif" />
    <Content Include="Images\Help.png" />
    <Content Include="Images\hyperlink.png" />
    <Content Include="Images\icn_undo_gray.gif" />
    <Content Include="Images\Imgmock\add.gif" />
    <Content Include="Images\Imgmock\addtocart.gif" />
    <Content Include="Images\Imgmock\arrowcurve.gif" />
    <Content Include="Images\Imgmock\blank.jpg" />
    <Content Include="Images\Imgmock\blankth.jpg" />
    <Content Include="Images\Imgmock\blue-right-arrow.gif" />
    <Content Include="Images\Imgmock\blue-trail-arrow.jpg" />
    <Content Include="Images\Imgmock\circle-bg-01-01.gif" />
    <Content Include="Images\Imgmock\circle-bg-01-02.gif" />
    <Content Include="Images\Imgmock\circle-bg-01-03.gif" />
    <Content Include="Images\Imgmock\circle-bg-02-01.gif" />
    <Content Include="Images\Imgmock\circle-bg-02-02.gif" />
    <Content Include="Images\Imgmock\circle-bg-02-03.gif" />
    <Content Include="Images\Imgmock\circle-bg-03-01.gif" />
    <Content Include="Images\Imgmock\circle-bg-03-02.gif" />
    <Content Include="Images\Imgmock\circle-bg-03-03.gif" />
    <Content Include="Images\Imgmock\circle-prod.gif" />
    <Content Include="Images\Imgmock\corner_se.gif" />
    <Content Include="Images\Imgmock\corner_sw.gif" />
    <Content Include="Images\Imgmock\crn-gn-lt.gif" />
    <Content Include="Images\Imgmock\crn-gn-rt.gif" />
    <Content Include="Images\Imgmock\dot_horiz.gif" />
    <Content Include="Images\Imgmock\dot_vert.gif" />
    <Content Include="Images\Imgmock\epluslogo.jpg" />
    <Content Include="Images\Imgmock\finished.jpg" />
    <Content Include="Images\Imgmock\go.gif" />
    <Content Include="Images\Imgmock\go.jpg" />
    <Content Include="Images\Imgmock\goall.gif" />
    <Content Include="Images\Imgmock\graph-type.gif" />
    <Content Include="Images\Imgmock\header_dontsee.gif" />
    <Content Include="Images\Imgmock\header_enter.gif" />
    <Content Include="Images\Imgmock\header_itemmatches.gif" />
    <Content Include="Images\Imgmock\header_search.gif" />
    <Content Include="Images\Imgmock\header_select.gif" />
    <Content Include="Images\Imgmock\header_select_cat.gif" />
    <Content Include="Images\Imgmock\leftarrow.gif" />
    <Content Include="Images\Imgmock\ML6320000A20000.gif" />
    <Content Include="Images\Imgmock\MS2550008A20000.gif" />
    <Content Include="Images\Imgmock\MX2460003A20000.gif" />
    <Content Include="Images\Imgmock\NOTAVAIL.GIF" />
    <Content Include="Images\Imgmock\printer.jpg" />
    <Content Include="Images\Imgmock\printernormal.jpg" />
    <Content Include="Images\Imgmock\refine.gif" />
    <Content Include="Images\Imgmock\resetone.gif" />
    <Content Include="Images\Imgmock\round_urbevel.gif" />
    <Content Include="Images\Imgmock\selectall.jpg" />
    <Content Include="Images\Imgmock\selectallnormal.jpg" />
    <Content Include="Images\Imgmock\spacer.gif" />
    <Content Include="Images\Imgmock\spot-buy.gif" />
    <Content Include="Images\Imgmock\table-type.gif" />
    <Content Include="Images\Imgmock\tan-button-go.gif" />
    <Content Include="Images\Imgmock\tan-button-go.jpg" />
    <Content Include="Images\Imgmock\tan-cart-icon.gif" />
    <Content Include="Images\Imgmock\tan-cart-icon.jpg" />
    <Content Include="Images\Imgmock\triW.gif" />
    <Content Include="Images\Imgmock\ttl-prod.gif" />
    <Content Include="Images\Imgmock\ulbevel.gif" />
    <Content Include="Images\Imgmock\umbevel.gif" />
    <Content Include="Images\Imgmock\unselectall.jpg" />
    <Content Include="Images\Imgmock\unselectallnormal.jpg" />
    <Content Include="Images\Imgmock\urbevel.gif" />
    <Content Include="Images\investor_relations.gif" />
    <Content Include="Images\investor_relations_on.gif" />
    <Content Include="Images\Jira_AnswerLab.png" />
    <Content Include="Images\kaman-logo.png" />
    <Content Include="Images\KamanIndustries.logo.bmp" />
    <Content Include="Images\knive.png" />
    <Content Include="Images\left.png" />
    <Content Include="Images\leftarrow.gif" />
    <Content Include="Images\LoginImg\core_values.png" />
    <Content Include="Images\LoginImg\Mexico.png" />
    <Content Include="Images\LoginImg\sdibannner.png" />
    <Content Include="Images\LoginImg\slider_txt_arrow.png" />
    <Content Include="Images\logoff.gif" />
    <Content Include="Images\logoff_on.gif" />
    <Content Include="Images\logon.gif" />
    <Content Include="Images\logon_on.gif" />
    <Content Include="Images\logoSDI_Vert.png" />
    <Content Include="Images\logoSDI_Vert_Mid.png" />
    <Content Include="Images\logoSDI_Vert_Mid1.png" />
    <Content Include="Images\logoSDI_Vert_Small.png" />
    <Content Include="Images\logo_honeywell.gif" />
    <Content Include="Images\Logo_Just SDI_color.jpg" />
    <Content Include="Images\logo_millercoors.bmp" />
    <Content Include="Images\log_off.gif" />
    <Content Include="Images\log_off_active.gif" />
    <Content Include="Images\log_on.gif" />
    <Content Include="Images\log_on_active.gif" />
    <Content Include="Images\manufacturing_hp.gif" />
    <Content Include="Images\manufacturing_hp_on.gif" />
    <Content Include="Images\MayerLogo_Full_BG_RGB.jpg" />
    <Content Include="Images\mcmaster.png" />
    <Content Include="Images\menu_dpdn_icon.png" />
    <Content Include="Images\metalfast-logo.jpg" />
    <Content Include="Images\Mexico.png" />
    <Content Include="Images\misumi.jpg" />
    <Content Include="Images\mombrands.jpg" />
    <Content Include="Images\MotionIndustries.logo.bmp" />
    <Content Include="Images\MRCGlobal.bmp" />
    <Content Include="Images\mro_defined.gif" />
    <Content Include="Images\mro_defined_on.gif" />
    <Content Include="Images\MSCDIRECT.JPG" />
    <Content Include="Images\nav_tile.gif" />
    <Content Include="Images\NBTY.gif" />
    <Content Include="Images\New-Radwell-Logo-high-res.jpg" />
    <Content Include="Images\next.gif" />
    <Content Include="Images\nextPage_hover.png" />
    <Content Include="Images\nextPage_normal.png" />
    <Content Include="Images\noimage_new.png" />
    <Content Include="Images\none.gif" />
    <Content Include="Images\OD_Horz.jpg" />
    <Content Include="Images\onesource165px_notag.jpg" />
    <Content Include="Images\our_customers.gif" />
    <Content Include="Images\our_customers_on.gif" />
    <Content Include="Images\paperclip.jpg" />
    <Content Include="Images\paperclip_small.jpg" />
    <Content Include="Images\pattern.png" />
    <Content Include="Images\pen1.jpg" />
    <Content Include="Images\preloader.gif" />
    <Content Include="Images\press_room.gif" />
    <Content Include="Images\press_room_on.gif" />
    <Content Include="Images\prevPage_hover.png" />
    <Content Include="Images\prevPage_normal.png" />
    <Content Include="Images\printSelected.gif" />
    <Content Include="Images\print_icon.gif" />
    <Content Include="Images\prologo.jpg" />
    <Content Include="Images\radwell.jpg" />
    <Content Include="Images\record_sucess.png" />
    <Content Include="Images\rejected.png" />
    <Content Include="Images\right.png" />
    <Content Include="Images\round_urbevel.gif" />
    <Content Include="Images\save-icon-btn.png" />
    <Content Include="Images\SaveIcon.png" />
    <Content Include="Images\sb_logo.gif" />
    <Content Include="Images\sdi-login-logo.png" />
    <Content Include="Images\SDI-NoImage-New.png" />
    <Content Include="Images\SDI-PowerdByZeus-Trans.png" />
    <Content Include="Images\sdi.NotAvail2.png" />
    <Content Include="Images\sdi4.PNG" />
    <Content Include="Images\SDIAccordion.png" />
    <Content Include="Images\sdibannner.png" />
    <Content Include="Images\SDIExchange_2018_Reversed.png" />
    <Content Include="Images\SDIFooter_Email.png" />
    <Content Include="Images\SDIgears_ani_0.gif" />
    <Content Include="Images\SDIgears_ani_1.gif" />
    <Content Include="Images\SDIgears_ani_2.gif" />
    <Content Include="Images\SDIgears_ani_3.gif" />
    <Content Include="Images\sdiInfo.png" />
    <Content Include="Images\SdiLogo-NoImage.png" />
    <Content Include="Images\sdilogo2.jpg" />
    <Content Include="Images\sdilogo3.jpg" />
    <Content Include="Images\SDILogo_Email.png" />
    <Content Include="Images\SDIminsIcon.png" />
    <Content Include="Images\SDIplsIcon.png" />
    <Content Include="Images\SDI_Icon512X512.jpg" />
    <Content Include="Images\SDI_Icon512X512_2.jpg" />
    <Content Include="Images\sdi_logo.gif" />
    <Content Include="Images\SDI_logo.jpg" />
    <Content Include="Images\SDI_logo.png" />
    <Content Include="Images\SDI_Logo2017_yellow.jpg" />
    <Content Include="Images\SDI_Logo2017_yellowhite.png" />
    <Content Include="Images\SDI_LOGO_ISOL.gif" />
    <Content Include="Images\SDI_Sprite.png" />
    <Content Include="Images\SDI_Sprite1.png" />
    <Content Include="Images\SDNewLogo_Email.png" />
    <Content Include="Images\SelectAll.gif" />
    <Content Include="Images\SetPermission.png" />
    <Content Include="Images\sherwin-williams.jpg" />
    <Content Include="Images\shopcart.png" />
    <Content Include="Images\shppng-pricetick.png" />
    <Content Include="Images\sidemenumock.jpg" />
    <Content Include="Images\slider_txt_arrow.png" />
    <Content Include="Images\slides\big_buck_bunny.jpg" />
    <Content Include="Images\slides\megamind1048.jpg" />
    <Content Include="Images\slides\megamind_07.jpg" />
    <Content Include="Images\slides\ratatouille2.jpg" />
    <Content Include="Images\slides\up-official-trailer-fake.jpg" />
    <Content Include="Images\slides\wall-e.jpg" />
    <Content Include="Images\slides\_notes\dwsync.xml" />
    <Content Include="Images\solutions_hp.gif" />
    <Content Include="Images\solutions_hp_on.gif" />
    <Content Include="Images\staples-logo-2019.jpg" />
    <Content Include="Images\Staples.logo.bmp" />
    <Content Include="Images\star.svg" />
    <Content Include="Images\submenu_dpdn_icon.png" />
    <Content Include="Images\success_stories_hp.gif" />
    <Content Include="Images\success_stories_on.gif" />
    <Content Include="Images\SupplierLogo\2093.png" />
    <Content Include="Images\SupplierLogo\26886.png" />
    <Content Include="Images\SupplierLogo\38573.png" />
    <Content Include="Images\SupplierLogo\39777.png" />
    <Content Include="Images\SupplierLogo\3M.png" />
    <Content Include="Images\SupplierLogo\41175.png" />
    <Content Include="Images\SupplierLogo\6.png" />
    <Content Include="Images\SupplierLogo\7789.png" />
    <Content Include="Images\SupplierLogo\AMAZON.png" />
    <Content Include="Images\SupplierLogo\CAPP.png" />
    <Content Include="Images\SupplierLogo\IMP.png" />
    <Content Include="Images\SupplierLogo\MSC.png" />
    <Content Include="Images\SupplierLogo\ORS.png" />
    <Content Include="Images\SupplierLogo\Stauffer.jpg" />
    <Content Include="Images\swagelok.png" />
    <Content Include="Images\TextFile\termsnconditionsdoc.txt" />
    <Content Include="Images\thermofisher.jpg" />
    <Content Include="Images\thumbs\big_buck_bunny.jpg" />
    <Content Include="Images\thumbs\megamind1048.jpg" />
    <Content Include="Images\thumbs\megamind_07.jpg" />
    <Content Include="Images\thumbs\ratatouille2.jpg" />
    <Content Include="Images\thumbs\up-official-trailer-fake.jpg" />
    <Content Include="Images\thumbs\wall-e.jpg" />
    <Content Include="Images\thumbs\_notes\dwsync.xml" />
    <Content Include="Images\thumb_hover.png" />
    <Content Include="Images\thumb_normal.png" />
    <Content Include="Images\thumb_selected.png" />
    <Content Include="Images\toggle-arrowDown.png" />
    <Content Include="Images\toggle-arrowUp.png" />
    <Content Include="Images\TopMock.jpg" />
    <Content Include="Images\Topsellers-heading.png" />
    <Content Include="Images\TXT.BMP" />
    <Content Include="Images\ulbevel.gif" />
    <Content Include="Images\uline-logo.png" />
    <Content Include="Images\umbevel.gif" />
    <Content Include="Images\UNCC.jpg" />
    <Content Include="Images\UnselectAll.gif" />
    <Content Include="Images\UPenn.jpg" />
    <Content Include="Images\urbevel.gif" />
    <Content Include="Images\VanMeter_0.jpg" />
    <Content Include="Images\veritiv-logo.gif" />
    <Content Include="Images\viewupdate.gif" />
    <Content Include="Images\vwdetail_noimage.png" />
    <Content Include="Images\vwr-new-logo.png" />
    <Content Include="Images\vwr_intl.jpg" />
    <Content Include="Images\waitAnima2.gif" />
    <Content Include="Images\Wallet-icon.png" />
    <Content Include="Images\Weinstein.JPG" />
    <Content Include="Images\XchanGe_Landing.png" />
    <Content Include="Images\Yellow-dropdown-arrow.png" />
    <Content Include="PunchOutcXML\APPLIED.PriceGrpIdentifier.xml" />
    <Content Include="PunchOutcXML\GHX.PriceGrpIdentifier.xml" />
    <Content Include="PunchOutcXML\Grainger.PriceGrpIdentifier.xml" />
    <Content Include="PunchOutcXML\GraybaR.PriceGrpIdentifier.xml" />
    <Content Include="PunchOutcXML\OFFICEDEPOT.PriceGrpIdentifier.xml" />
    <Content Include="PunchOutcXML\orig.PunchOut.xml" />
    <Content Include="PunchOutcXML\PunchOut.xml" />
    <Content Include="PunchOutcXML\punchOutSetupRequestTemplate.xml" />
    <Content Include="PunchOutcXML\VWRINTL.PriceGrpIdentifier.xml" />
    <Content Include="PunchOutFiles\Sample.js" />
    <Content Include="ReferenceAssemblies\configSetting.xml" />
    <Content Include="ReferenceDll\CrystalDecisions.CrystalReports.Engine.dll" />
    <Content Include="ReferenceDll\CrystalDecisions.Shared.dll" />
    <Content Include="ReferenceDll\SDI.PunchOut.dll" />
    <Content Include="ReferenceDll\UpdEmailOut.dll" />
    <Content Include="VendorXML\VendorXREF.xml" />
    <Content Include="Images\SDiExchange_com_Manual.pdf" />
    <Content Include="Images\Vendor Portal FAQ.pdf" />
    <Content Include="Images\Zep Aerosols.pdf" />
    <None Include="Connected Services\sdiFileUploader\fileUploader.wsdl" />
    <Content Include="Connected Services\sdiFileUploader\SDI.Walmart.API.sdiFileUploader.uploadFileResponse.datasource">
      <DependentUpon>Reference.svcmap</DependentUpon>
    </Content>
    <Content Include="Crystal Reports\CartConfirmReport_Asset.rpt" />
    <Content Include="Crystal Reports\CartConfirmReport.rpt" />
    <None Include="My Project\PublishProfiles\FolderProfile1.pubxml" />
    <Content Include="Web References\com.tf7.www1\EquipmentHistoryItem.datasource">
      <DependentUpon>Reference.map</DependentUpon>
    </Content>
    <Content Include="Web References\com.tf7.www1\EquipmentInfo.datasource">
      <DependentUpon>Reference.map</DependentUpon>
    </Content>
    <Content Include="Web References\com.tf7.www1\EquipmentTypeInfo.datasource">
      <DependentUpon>Reference.map</DependentUpon>
    </Content>
    <Content Include="Web References\com.tf7.www1\InstallationInfo.datasource">
      <DependentUpon>Reference.map</DependentUpon>
    </Content>
    <Content Include="Web References\com.tf7.www1\PropertyMapping.datasource">
      <DependentUpon>Reference.map</DependentUpon>
    </Content>
    <Content Include="Web References\com.tf7.www1\PropertyUnitInfo.datasource">
      <DependentUpon>Reference.map</DependentUpon>
    </Content>
    <Content Include="Web References\com.tf7.www1\Reference.map">
      <Generator>MSDiscoCodeGenerator</Generator>
      <LastGenOutput>Reference.vb</LastGenOutput>
    </Content>
    <Content Include="Web References\com.tf7.www1\RemovalInfo.datasource">
      <DependentUpon>Reference.map</DependentUpon>
    </Content>
    <None Include="Web References\com.tf7.www1\TangoEquipment.disco" />
    <Content Include="Web References\com.tf7.www\CustomerInfo.datasource">
      <DependentUpon>Reference.map</DependentUpon>
    </Content>
    <Content Include="Web References\com.tf7.www\Reference.map">
      <Generator>MSDiscoCodeGenerator</Generator>
      <LastGenOutput>Reference.vb</LastGenOutput>
    </Content>
    <None Include="Web References\com.tf7.www1\TangoEquipment.wsdl" />
    <Content Include="Web References\com.tf7.www1\VendorItem.datasource">
      <DependentUpon>Reference.map</DependentUpon>
    </Content>
    <None Include="Web References\com.tf7.www\TangoUser.wsdl" />
    <Content Include="Web References\com.tf7.www\UriTypeItem.datasource">
      <DependentUpon>Reference.map</DependentUpon>
    </Content>
    <Content Include="Web References\com.tf7.www\UserInfo.datasource">
      <DependentUpon>Reference.map</DependentUpon>
    </Content>
    <None Include="Web References\EmailServices\EmailServices.disco" />
    <None Include="Web References\com.tf7.www\TangoUser.disco" />
    <Content Include="Web.config">
      <SubType>Designer</SubType>
    </Content>
  </ItemGroup>
  <ItemGroup>
    <Compile Include="App_Start\RouteConfig.vb" />
    <Compile Include="App_Start\WebApiConfig.vb" />
    <Compile Include="Connected Services\sdiFileUploader\Reference.vb">
      <AutoGen>True</AutoGen>
      <DesignTime>True</DesignTime>
      <DependentUpon>Reference.svcmap</DependentUpon>
    </Compile>
    <Compile Include="Controllers\API\SDIAPIController.vb" />
    <Compile Include="Controllers\API\SDIFileController.vb" />
    <Compile Include="Controllers\API\SDIMobileController.vb" />
    <Compile Include="Controllers\API\SDIMobileSecureController.vb" />
    <Compile Include="Controllers\API\SDIPunchoutController.vb" />
    <Compile Include="Controllers\API\SDISecureController.vb" />
    <Compile Include="Controllers\API\SDISecureAsyncController.vb" />
    <Compile Include="Controllers\API\SDITimeOutController.vb" />
    <Compile Include="Controllers\BL\AuthenticationBL.vb" />
    <Compile Include="Controllers\BL\ChangePasswordBL.vb" />
    <Compile Include="Controllers\BL\FavItemMainBL.vb" />
    <Compile Include="Controllers\BL\FavOrderMainBL.vb" />
    <Compile Include="Controllers\BL\ForgotPasswordBL.vb" />
    <Compile Include="Controllers\BL\HomepageBL.vb" />
    <Compile Include="Controllers\BL\ItemDetailNewBL.vb" />
    <Compile Include="Controllers\BL\MenuBL.vb" />
    <Compile Include="Controllers\BL\MobileAPIEndpointsBL.vb" />
    <Compile Include="Controllers\BL\NonCatalogBL.vb" />
    <Compile Include="Controllers\BL\OrderApprovalBL.vb" />
    <Compile Include="Controllers\BL\OrderStatusBL.vb" />
    <Compile Include="Controllers\BL\PassThroughAutheBL.vb" />
    <Compile Include="Controllers\BL\ProfileBL.vb" />
    <Compile Include="Controllers\BL\PunchoutBL.vb" />
    <Compile Include="Controllers\BL\RequestorApprovalBL.vb" />
    <Compile Include="Controllers\BL\SearchpageBL.vb" />
    <Compile Include="Controllers\BL\ServiceChannelBL.vb" />
    <Compile Include="Controllers\BL\ShoppingCartBL.vb" />
    <Compile Include="Controllers\BL\ShopRedirectBL.vb" />
    <Compile Include="Controllers\BL\UserNotification.vb" />
    <Compile Include="Controllers\BL\UserNotificationBO.vb" />
    <Compile Include="Controllers\BL\UserProfileBL.vb" />
    <Compile Include="Controllers\BL\waitingOrderStatus.vb" />
    <Compile Include="Controllers\BL\WaitingRequestorApprovalBL.vb" />
    <Compile Include="Controllers\BO\ChangePasswordBO.vb" />
    <Compile Include="Controllers\BO\constants.vb" />
    <Compile Include="Controllers\BO\FavItemMainBO.vb" />
    <Compile Include="Controllers\BO\FavOrderMainBO.vb" />
    <Compile Include="Controllers\BO\ForgotPasswordBO.vb" />
    <Compile Include="Controllers\BO\HomepageBO.vb" />
    <Compile Include="Controllers\BO\ItemDetailNewBO.vb" />
    <Compile Include="Controllers\BO\LoginBO.vb" />
    <Compile Include="Controllers\BO\MenuBO.vb" />
    <Compile Include="Controllers\BO\MessageBO.vb" />
    <Compile Include="Controllers\BO\MobileAppConfigsBO.vb" />
    <Compile Include="Controllers\BO\MobileCartBOs\FileUploadRequest.vb" />
    <Compile Include="Controllers\BO\MobileCartBOs\LoginDetails.vb" />
    <Compile Include="Controllers\BO\MobileCartBOs\OrderDetails.vb" />
    <Compile Include="Controllers\BO\MobileCartBOs\OrderLineItems.vb" />
    <Compile Include="Controllers\BO\MobileCartBOs\UserProfile.vb" />
    <Compile Include="Controllers\BO\NonCatalogBO.vb" />
    <Compile Include="Controllers\BO\OrderApprovalBO.vb" />
    <Compile Include="Controllers\BO\OrderStatusBO.vb" />
    <Compile Include="Controllers\BO\PassThroughAutheBO.vb" />
    <Compile Include="Controllers\BO\PunchoutBO.vb" />
    <Compile Include="Controllers\BO\RequestorApprovalBO.vb" />
    <Compile Include="Controllers\BO\sdiItemPrice.vb" />
    <Compile Include="Controllers\BO\sdiItemStockType.vb" />
    <Compile Include="Controllers\BO\SearchPageBO.vb" />
    <Compile Include="Controllers\BO\ShoppingCartBO.vb" />
    <Compile Include="Controllers\BO\SupplierOrderEntryBO.vb" />
    <Compile Include="Controllers\BO\UserProfileBO.vb" />
    <Compile Include="Controllers\BO\waitingOrderApprovalBO.vb" />
    <Compile Include="Controllers\BO\WaitingRequestorApprovalBO.vb" />
    <Compile Include="Controllers\DAL\Orbdbdata.vb" />
    <Compile Include="Controllers\DAL\SQLDBData.vb" />
    <Compile Include="Controllers\BO\appContainer.vb" />
    <Compile Include="Controllers\Utilities\ApplicationLogger.vb" />
    <Compile Include="Controllers\BO\appLoad.vb" />
    <Compile Include="Controllers\BO\appStop.vb" />
    <Compile Include="Controllers\Utilities\ApproveOrder.vb" />
    <Compile Include="Controllers\Utilities\BuildMenu.vb" />
    <Compile Include="Controllers\Utilities\CartConfirm.vb" />
    <Compile Include="Controllers\Utilities\clsAccessPrivileges.vb" />
    <Compile Include="Controllers\Utilities\clsApprovalHistory.vb" />
    <Compile Include="Controllers\Utilities\clsCrystalReports.vb" />
    <Compile Include="Controllers\Utilities\clsEmplTbl.vb" />
    <Compile Include="Controllers\Utilities\clsEnterprise.vb" />
    <Compile Include="Controllers\Utilities\clsFavorites.vb" />
    <Compile Include="Controllers\Utilities\clsInvItemID.vb" />
    <Compile Include="Controllers\Utilities\clsOrder_req.vb" />
    <Compile Include="Controllers\Utilities\clsPasswords.vb" />
    <Compile Include="Controllers\Utilities\clsProgramMaster.vb" />
    <Compile Include="Controllers\Utilities\clsPunchin.vb" />
    <Compile Include="Controllers\Utilities\clsRole.vb" />
    <Compile Include="Controllers\Utilities\clsSDIAudit.vb" />
    <Compile Include="Controllers\Utilities\clsSDITrack.vb" />
    <Compile Include="Controllers\Utilities\clsSerialItem.vb" />
    <Compile Include="Controllers\Utilities\clsshoppingcart.vb" />
    <Compile Include="Controllers\BO\clsSignature.vb" />
    <Compile Include="Controllers\Utilities\clsSndTcketEml.vb" />
    <Compile Include="Controllers\Utilities\clsUserTbl.vb" />
    <Compile Include="Controllers\Utilities\ClsUsrCtrlPerm.vb" />
    <Compile Include="Controllers\Utilities\containerTypeCounter.vb" />
    <Compile Include="Controllers\BO\Custom.vb" />
    <Compile Include="Controllers\Utilities\Encryption64.vb" />
    <Compile Include="Controllers\Utilities\ExceptionHelper.vb" />
    <Compile Include="Controllers\Utilities\GenFunctions.vb" />
    <Compile Include="Controllers\Utilities\NonCatalogSearchyAPI.vb" />
    <Compile Include="Controllers\Utilities\NotifyClass.vb" />
    <Compile Include="Controllers\Utilities\OrderApprovals.vb" />
    <Compile Include="Controllers\Utilities\PredictiveSearch.vb" />
    <Compile Include="Controllers\Utilities\PurchaseHistory.vb" />
    <Compile Include="Controllers\Utilities\Reference.vb" />
    <Compile Include="Controllers\Utilities\sdiCommon.vb" />
    <Compile Include="Controllers\Utilities\sdiConversionRate.vb" />
    <Compile Include="Controllers\Utilities\sdiCurrency.vb" />
    <Compile Include="Controllers\BO\sdiItem.vb" />
    <Compile Include="Controllers\Utilities\sdiFileUploadResult.vb" />
    <Compile Include="Controllers\Utilities\sdiMultiCurrency.vb" />
    <Compile Include="Controllers\BO\signAttrib.vb" />
    <Compile Include="Controllers\Utilities\TransactionHistory.vb" />
    <Compile Include="Controllers\Utilities\UnilogORDBData.vb" />
    <Compile Include="Controllers\Utilities\UnilogSearch.vb" />
    <Compile Include="Controllers\Utilities\UpgradeMenuStructure.vb" />
    <Compile Include="Controllers\Utilities\VendorInfo.vb" />
    <Compile Include="Controllers\Utilities\VoucherClass.vb" />
    <Compile Include="Controllers\Utilities\WebPSharedFunc.vb" />
    <Compile Include="Global.asax.vb">
      <DependentUpon>Global.asax</DependentUpon>
    </Compile>
    <Compile Include="Interface\IMessageReceiver.vb" />
    <Compile Include="Models\ClientMasterRepository.vb" />
    <Compile Include="Models\Helper.vb" />
    <Compile Include="Models\MyAuthorizationServerProvider.vb" />
    <Compile Include="Models\RefreshToken.vb" />
    <Compile Include="Models\RefreshTokenProvider.vb" />
    <Compile Include="Models\UserMasterRepository.vb" />
    <Compile Include="My Project\AssemblyInfo.vb" />
    <Compile Include="My Project\Application.Designer.vb">
      <AutoGen>True</AutoGen>
      <DependentUpon>Application.myapp</DependentUpon>
    </Compile>
    <Compile Include="My Project\MyExtensions\MyWebExtension.vb">
      <VBMyExtensionTemplateID>Microsoft.VisualBasic.Web.MyExtension</VBMyExtensionTemplateID>
      <VBMyExtensionTemplateVersion>1.0.0.0</VBMyExtensionTemplateVersion>
    </Compile>
    <Compile Include="My Project\Resources.Designer.vb">
      <AutoGen>True</AutoGen>
      <DesignTime>True</DesignTime>
      <DependentUpon>Resources.resx</DependentUpon>
    </Compile>
    <Compile Include="My Project\Settings.Designer.vb">
      <AutoGen>True</AutoGen>
      <DependentUpon>Settings.settings</DependentUpon>
      <DesignTimeSharedInput>True</DesignTimeSharedInput>
    </Compile>
    <Compile Include="Startup.vb" />
    <Compile Include="Web References\com.tf7.www1\Reference.vb">
      <AutoGen>True</AutoGen>
      <DesignTime>True</DesignTime>
      <DependentUpon>Reference.map</DependentUpon>
    </Compile>
    <Compile Include="Web References\com.tf7.www\Reference.vb">
      <AutoGen>True</AutoGen>
      <DesignTime>True</DesignTime>
      <DependentUpon>Reference.map</DependentUpon>
    </Compile>
    <Compile Include="Web References\EmailServices\Reference.vb">
      <AutoGen>True</AutoGen>
      <DesignTime>True</DesignTime>
      <DependentUpon>Reference.map</DependentUpon>
    </Compile>
  </ItemGroup>
  <ItemGroup>
    <EmbeddedResource Include="My Project\Resources.resx">
      <Generator>VbMyResourcesResXFileCodeGenerator</Generator>
      <LastGenOutput>Resources.Designer.vb</LastGenOutput>
      <CustomToolNamespace>My.Resources</CustomToolNamespace>
      <SubType>Designer</SubType>
    </EmbeddedResource>
  </ItemGroup>
  <ItemGroup>
    <Content Include="Crystal Reports\CartConfirmReport_Old.rpt" />
    <Content Include="Images\!NYCDOE Catalogs_Errata SheetToolbox Prices.pdf" />
    <Content Include="Images\!NYCDOE Z6000 Parts-Installation.pdf" />
    <Content Include="Images\!NYCDOE Z6100 Parts-Installation.pdf" />
    <Content Include="Images\arrow_left_sdi.psp" />
    <Content Include="Images\Attention_Suppliers_Voucher_Entry.pdf" />
    <Content Include="Images\FY15 Supplies Catalog.pdf" />
    <Content Include="Images\FY15 Toolbox Catalog.pdf" />
    <Content Include="Images\NYCDOE_Catalogue_Errata_Sheet.pdf" />
    <Content Include="Images\pspbrwse.jbf" />
    <Content Include="Images\SDIASNPOCONFIRMATIONSOP.pdf" />
    <Content Include="Images\SDI_Sprite1.psd" />
    <Content Include="Images\slides\.DS_Store" />
    <Content Include="Images\thumbs\.DS_Store" />
    <None Include="My Project\Application.myapp">
      <Generator>MyApplicationCodeGenerator</Generator>
      <LastGenOutput>Application.Designer.vb</LastGenOutput>
    </None>
    <None Include="My Project\PublishProfiles\FolderProfile.pubxml" />
    <None Include="My Project\Settings.settings">
      <Generator>SettingsSingleFileGenerator</Generator>
      <CustomToolNamespace>My</CustomToolNamespace>
      <LastGenOutput>Settings.Designer.vb</LastGenOutput>
    </None>
    <Content Include="Views\web.config">
      <SubType>Designer</SubType>
    </Content>
    <None Include="packages.config" />
    <Content Include="PunchOutcXML\cXML.dtd" />
    <Content Include="ReferenceDll\UpdEmailOut.dll.config" />
    <None Include="Web References\EmailServices\EmailServices.wsdl" />
    <Content Include="Web References\EmailServices\Reference.map">
      <Generator>MSDiscoCodeGenerator</Generator>
      <LastGenOutput>Reference.vb</LastGenOutput>
    </Content>
    <None Include="Web.Debug.config">
      <DependentUpon>Web.config</DependentUpon>
    </None>
    <None Include="Web.Release.config">
      <DependentUpon>Web.config</DependentUpon>
    </None>
  </ItemGroup>
  <ItemGroup>
    <Folder Include="App_Data\" />
  </ItemGroup>
  <ItemGroup>
    <WCFMetadata Include="Connected Services\" />
  </ItemGroup>
  <ItemGroup>
    <WebReferences Include="Web References\" />
  </ItemGroup>
  <ItemGroup>
    <WebReferenceUrl Include="http://sdixaws2016test:8083/SDIEmailSvc/EmailServices.asmx">
      <UrlBehavior>Dynamic</UrlBehavior>
      <RelPath>Web References\EmailServices\</RelPath>
      <UpdateFromURL>http://sdixaws2016test:8083/SDIEmailSvc/EmailServices.asmx</UpdateFromURL>
      <ServiceLocationURL>
      </ServiceLocationURL>
      <CachedDynamicPropName>
      </CachedDynamicPropName>
      <CachedAppSettingsObjectName>MySettings</CachedAppSettingsObjectName>
      <CachedSettingsPropName>SDI_Walmart_API_EmailServices_EmailServices</CachedSettingsPropName>
    </WebReferenceUrl>
    <WebReferenceUrl Include="https://www.tf7.com/Concert/TangoEquipment.asmx">
      <UrlBehavior>Dynamic</UrlBehavior>
      <RelPath>Web References\com.tf7.www1\</RelPath>
      <UpdateFromURL>https://www.tf7.com/Concert/TangoEquipment.asmx</UpdateFromURL>
      <ServiceLocationURL>
      </ServiceLocationURL>
      <CachedDynamicPropName>
      </CachedDynamicPropName>
      <CachedAppSettingsObjectName>MySettings</CachedAppSettingsObjectName>
      <CachedSettingsPropName>SDI_Walmart_API_com_tf7_www1_TangoEquipment</CachedSettingsPropName>
    </WebReferenceUrl>
    <WebReferenceUrl Include="https://www.tf7.com/Concert/TangoUser.asmx">
      <UrlBehavior>Dynamic</UrlBehavior>
      <RelPath>Web References\com.tf7.www\</RelPath>
      <UpdateFromURL>https://www.tf7.com/Concert/TangoUser.asmx</UpdateFromURL>
      <ServiceLocationURL>
      </ServiceLocationURL>
      <CachedDynamicPropName>
      </CachedDynamicPropName>
      <CachedAppSettingsObjectName>MySettings</CachedAppSettingsObjectName>
      <CachedSettingsPropName>SDI_Walmart_API_com_tf7_www_TangoUser</CachedSettingsPropName>
    </WebReferenceUrl>
  </ItemGroup>
  <ItemGroup>
    <WCFMetadataStorage Include="Connected Services\sdiFileUploader\" />
  </ItemGroup>
  <PropertyGroup>
    <VisualStudioVersion Condition="'$(VisualStudioVersion)' == ''">10.0</VisualStudioVersion>
    <VSToolsPath Condition="'$(VSToolsPath)' == ''">$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)</VSToolsPath>
  </PropertyGroup>
  <PropertyGroup>
    <OptionExplicit>On</OptionExplicit>
  </PropertyGroup>
  <PropertyGroup>
    <OptionCompare>Binary</OptionCompare>
  </PropertyGroup>
  <PropertyGroup>
    <OptionStrict>Off</OptionStrict>
  </PropertyGroup>
  <PropertyGroup>
    <OptionInfer>On</OptionInfer>
  </PropertyGroup>
  <Import Project="$(MSBuildBinPath)\Microsoft.VisualBasic.targets" />
  <Import Project="$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets" Condition="'$(VSToolsPath)' != ''" />
  <Import Project="$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v10.0\WebApplications\Microsoft.WebApplication.targets" Condition="false" />
  <ProjectExtensions>
    <VisualStudio>
      <FlavorProperties GUID="{349c5851-65df-11da-9384-00065b846f21}">
        <WebProjectProperties>
          <UseIIS>True</UseIIS>
          <AutoAssignPort>True</AutoAssignPort>
          <DevelopmentServerPort>59670</DevelopmentServerPort>
          <DevelopmentServerVPath>/</DevelopmentServerVPath>
          <IISUrl>http://localhost:59670/</IISUrl>
          <NTLMAuthentication>False</NTLMAuthentication>
          <UseCustomServer>False</UseCustomServer>
          <CustomServerUrl>
          </CustomServerUrl>
          <SaveServerSettingsInUserFile>False</SaveServerSettingsInUserFile>
        </WebProjectProperties>
      </FlavorProperties>
    </VisualStudio>
  </ProjectExtensions>
  <Import Project="..\packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.3.6.0\build\net46\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.targets" Condition="Exists('..\packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.3.6.0\build\net46\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.targets')" />
  <Target Name="EnsureNuGetPackageBuildImports" BeforeTargets="PrepareForBuild">
    <PropertyGroup>
      <ErrorText>This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is {0}.</ErrorText>
    </PropertyGroup>
    <Error Condition="!Exists('..\packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.3.6.0\build\net46\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.targets')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.3.6.0\build\net46\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.targets'))" />
  </Target>
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name="BeforeBuild">
  </Target>
  <Target Name="AfterBuild">
  </Target>
  -->
</Project>