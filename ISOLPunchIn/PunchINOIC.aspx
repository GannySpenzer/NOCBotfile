<%@ Page Language="vb" AutoEventWireup="false" Codebehind="PunchINOIC.aspx.vb" Inherits="ISOLPunchIn.PunchINOIC"%>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
<HTML>
	<HEAD runat="server" id="HeadOIC">
		<meta http-equiv="X-UA-Compatible" content="IE=EmulateIE7" />
		<title>PunchInOIC Data Process</title>
		<meta name="GENERATOR" content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" content="Visual Basic .NET 7.1">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<LINK href="StyleSheet.css" type="text/css" rel="stylesheet">
		<LINK href="SDI.css" type="text/css" rel="stylesheet">
		<LINK href="menuStyle.css" type="text/css" rel="stylesheet">
		<script src="http://www.google-analytics.com/urchin.js" type="text/javascript"></script>
		<script type="text/javascript">_uacct = "UA-161119-1"; urchinTracker();</script>
	</HEAD>
	<body MS_POSITIONING="GridLayout">
		<form id="frmLaunchPunchIN" method="post" runat="server">
			<TABLE id='BannerTable1' cellSpacing='0' cellPadding='0' width='100%' border='0'>
				<TR vAlign='top'>
					<TD style='WIDTH: 39.75%'>
						<a id='BuildBanner22_linkSDI' href='http://www.sdi.com'><img src='images/SDI_logo.jpg' alt='' border='0'></a></TD>
				</TR>
			</TABLE>
			<TABLE id='BannerTable2' style='WIDTH: 100%' cellSpacing='1' cellPadding='1' border='0'>
				<tr>
					<td class='TabBg' align='right' bgColor='#273d8c'>
						<IMG id='Banner__ctl4_Spacer' style='WIDTH: 1px; HEIGHT: 4px' alt='' src='images/1x1.gif'
							border='0'></td>
				</tr>
			</TABLE>
			<table cellspacing='0' cellpadding='0' border='0'>
				<tr>
					<td colspan="2" height="100">&nbsp;</td>
				</tr>
				<tr>
					<td width="133">&nbsp;</td>
					<td align="center">
						<font color="#273d8c" size="7"><span id="lblSDI">SDI</span> </SPAN></font><br>
						<font color="#b9d52b" size="6"><span id="lblISOL">InSite Online Catalog Load</span> 
							</SPAN></font><br>
						<br>
						<br>
						<font color="red" size="5"><span id="Message">
								<asp:Label ID="lblError" runat="server"></asp:Label></span> </font>
					</td>
				</tr>
				<TR>
					<TD width="133"></TD>
					<TD align="center">&nbsp;</TD>
				</TR>
				<TR>
					<TD width="133"></TD>
					<TD align="center">
						<asp:Button ID="btnReturn" runat="server" Text="Return" CssClass="button-fancy1" Visible="False"></asp:Button></TD>
				</TR>
			</table>
		</form>
	</body>
</HTML>
