<%@ Register TagPrefix="uc1" TagName="BuildBanner2" Src="BuildBanner2.ascx" %>
<%@ Page Language="vb" AutoEventWireup="false" Codebehind="DBErrorPage.aspx.vb" Inherits="ISOLPunchIn.DBErrorPage" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD>
		<title>DB Connection Error Page</title>
		<meta content="Microsoft Visual Studio.NET 7.0" name="GENERATOR">
		<meta content="Visual Basic 7.0" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<LINK href="StyleSheet.css" type="text/css" rel="stylesheet">
		<LINK href="SDI.css" type="text/css" rel="stylesheet">
		<LINK href="menuStyle.css" type="text/css" rel="stylesheet">
		<SCRIPT language="javascript" src="GenJSFunc.js"></SCRIPT>
		<script src="http://www.google-analytics.com/urchin.js" type="text/javascript"></script>
		<script type="text/javascript">_uacct = "UA-161119-1"; urchinTracker();</script>
	</HEAD>
	<body MS_POSITIONING="GridLayout">
		<form id="frmDBError" method="post" runat="server">
			<uc1:buildbanner2 id="BuildBanner2" runat="server" DisplayTitle="In-Site® Online DBError Page" FormTitle="DBError Page"></uc1:buildbanner2>
			<asp:Label id="lblDBError" style="Z-INDEX: 101; POSITION: absolute; TOP: 240px; LEFT: 192px"
				runat="server" Width="592px" Height="176px"></asp:Label>
		</form>
	</body>
</HTML>
