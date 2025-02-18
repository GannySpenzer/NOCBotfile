<%@ Control Language="vb" AutoEventWireup="false" Codebehind="BuildBanner2.ascx.vb" Inherits="ISOLPunchIn.BuildBanner2" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<LINK href="StyleSheet.css" type="text/css" rel="stylesheet">
<LINK href="SDI.css" type="text/css" rel="stylesheet">
<LINK href="menuStyle.css" type="text/css" rel="stylesheet">
<script type="text/javascript" src="CSScriptLib.js"></script>
<script type="text/javascript"><!--

//detect browser:
browserName = navigator.appName;
browserVer = parseInt(navigator.appVersion);
if (browserName == "Netscape" && browserVer >= 3) browserVer = "1";
else if (browserName == "Microsoft Internet Explorer" && browserVer == 4) browserVer = "1";
else browserVer = "2";

//preload images:
if (browserVer == 1) {
company1 = new Image();
company1.src = "images/company.gif";
company2 = new Image();
company2.src = "images/company_on.gif";
solutions_hp1 = new Image();
solutions_hp1.src = "images/solutions_hp.gif";
solutions_hp2 = new Image();
solutions_hp2.src = "images/solutions_hp_on.gif";
manufacturing_hp1 = new Image();
manufacturing_hp1.src = "images/manufacturing_hp.gif";
manufacturing_hp2 = new Image();
manufacturing_hp2.src = "images/manufacturing_hp_on.gif";
facilities_main_hp1 = new Image();
facilities_main_hp1.src = "images/facilities_main_hp.gif";
facilities_main_hp2 = new Image();
facilities_main_hp2.src = "images/facilities_hp_on.gif";
global_reach_hp1 = new Image();
global_reach_hp1.src = "images/global_reach_hp.gif";
global_reach_hp2 = new Image();
global_reach_hp2.src = "images/global_reach_on.gif";
success_stories_hp1 = new Image();
success_stories_hp1.src = "images/success_stories_hp.gif";
success_stories_hp2 = new Image();
success_stories_hp2.src = "images/success_stories_on.gif";
contact_us1 = new Image();
contact_us1.src = "images/contact_us.gif";
contact_us2 = new Image();
contact_us2.src = "images/contact_us_on.gif";
log_on1 = new Image();
log_on1.src = "images/logon.gif";
log_on2 = new Image();
log_on2.src = "images/logon_on.gif";
log_off1 = new Image();
log_off1.src = "images/logoff.gif";
log_off2 = new Image();
log_off2.src = "images/logoff_on.gif";
}

// image swapping function:
function hiLite(imgDocID, imgObjName, comment) {
	if (browserVer == 1) {
		document.images[imgDocID].src = eval(imgObjName + ".src");
		window.status = comment; return true;
	}}

// --></script>
<TABLE id="Table1" cellSpacing="0" cellPadding="0" width="100%" border="0">
	<TR valign="top">
		<TD style="WIDTH: 52%"><img src="images/SDI_logo.jpg" alt="SDI Logo" height="80" width="200" border="0"></TD>
		
		
	</TR>
</TABLE>
<TABLE id="BannerTable2" style="WIDTH: 100%" cellSpacing="1" cellPadding="1" border="0">
	<tr>
		<TD class="BannerTitle"></TD>
		<TD class="BannerTitle"><%=DisplayTitle%></TD>
		<TD class="BannerTitle" align="right"><%=session("CONAME")%></TD>
	</tr>
</TABLE>

<TABLE id="TableTabs2" cellSpacing="0" cellPadding="0" width="100%" border="0">
	<% if NOT TABS="N" then %>
	<TR>
		<TD valign="bottom" nowrap>
			<table cellSpacing="0" cellPadding="0" border="0" width="100%">
				<tr>
					<td width="100%" background="images/nav_tile.gif">
						<table border="0" cellspacing="0" cellpadding="0" background="images/nav_tile.gif">
							<tr>
								<td width="216" bgcolor="white"><IMG height="27" alt="" src="images/clear.gif" width="216" border="0"></td>
								<td width="74"><A onmouseover="hiLite('company','company2','')" onmouseout="hiLite('company','company1','')"
										href="http://www.sdi.com/company/company_index.html"><IMG height="30" src="images/company.gif" width="86" border="0" name="company" alt="Company"></A></td>
								<td width="65"><A onmouseover="hiLite('solutions_hp','solutions_hp2','')" onmouseout="hiLite('solutions_hp','solutions_hp1','')"
										href="http://www.sdi.com/solutions/solutions_index.html"><IMG height="30" src="images/solutions_hp.gif" width="69" border="0" name="solutions_hp"
											alt="Solutions"></A></td>
								<td width="88"><A onmouseover="hiLite('manufacturing_hp','manufacturing_hp2','')" onmouseout="hiLite('manufacturing_hp','manufacturing_hp1','')"
										href="http://www.sdi.com/manufacturing/manufacturing_index.html"><IMG height="30" src="images/manufacturing_hp.gif" width="98" border="0" name="manufacturing_hp"
											alt="Manufacturing"></A></td>
								<td width="138"><A onmouseover="hiLite('facilities_main_hp','facilities_main_hp2','')" onmouseout="hiLite('facilities_main_hp','facilities_main_hp1','')"
										href="http://www.sdi.com/facilities_maint/facilities_maint-index.html"><IMG height="30" src="images/facilities_main_hp.gif" width="138" border="0" name="facilities_main_hp"
											alt="Facilities Maintenance"></A></td>
								<td width="76"><A onmouseover="hiLite('global_reach_hp','global_reach_hp2','')" onmouseout="hiLite('global_reach_hp','global_reach_hp1','')"
										href="http://www.sdi.com/global_reach/global_reach-index.html"><IMG height="30" src="images/global_reach_hp.gif" width="88" border="0" name="global_reach_hp"
											alt="Global Reach"></A></td>
								<td width="102"><A onmouseover="hiLite('success_stories_hp','success_stories_hp2','')" onmouseout="hiLite('success_stories_hp','success_stories_hp1','')"
										href="http://www.sdi.com/success_stories/success-stories_index.html"><IMG height="30" src="images/success_stories_hp.gif" width="102" border="0" name="success_stories_hp"
											alt="Success Stories"></A></td>
								<!-- <td width="74"><a 
            onmouseover="hiLite('contact_us','contact_us2','')" 
            onmouseout="hiLite('contact_us','contact_us1','')" 
            href="<%=strmailto%>"><IMG height="29" src="images/contact_us.gif" width="74" border="0" name="contact_us"></a></td>-->
								<% if session("USERID") = "" then %>
								<td width="74"><A onmouseover="hiLite('log_on','log_on2','')" onmouseout="hiLite('log_on','log_on1','')"
										href="javascript:alert('Already logged on')"><IMG id="log_on" height="30" alt="Insiteonline" src="images/logon.gif" width="74" border="0"
											name="log_on"></A></td>
								<% else %>
								<% If Request.ServerVariables("HTTP_HOST").ToUpper = "LOCALHOST" or Request.ServerVariables("HTTP_HOST").ToUpper = "CPTEST.SDI.COM"%>
								<td width="74"><A onmouseover="hiLite('log_off','log_off2','')" onmouseout="hiLite('log_off','log_off1','')"
										href="../Insiteonline/logoff.aspx"><IMG id="log_offA" height="30" alt="Insiteonline" src="images/logoff.gif" width="74"
											border="0" name="log_off"></A></td>
								<% else %>
								<td width="74"><A onmouseover="hiLite('log_off','log_off2','')" onmouseout="hiLite('log_off','log_off1','')"
										href="../logoff.aspx"><IMG id="log_offB" height="30" alt="Insiteonline" src="images/logoff.gif" width="74"
											border="0" name="log_off"></A></td>
								<% end if %>
								<% end if %>
							</tr>
						</table>
					</td>
				</tr>
				<% end if %>
				<tr>
					<td align="right" bgcolor="#273d8c" class="TabBg"><img id="Banner__ctl4_Spacer" src="images/1x1.gif" alt="" border="0" style="WIDTH:1px;HEIGHT:4px"></td>
				</tr>
			</table>
			<script><asp:Literal id="ltlAlert" runat="server" EnableViewState="False"></asp:Literal></script>
	</TR>
</TABLE>
