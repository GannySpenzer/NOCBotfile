<%@ Page Language="VB" AutoEventWireup="false" Inherits="Insiteonline.OrderItems" CodeBehind="OrderItems.aspx.vb" MasterPageFile="~/MasterPage/SDIXMaster.Master" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript" src="https://www.google.com/jsapi"></script>

    <title>Purchasing Cart</title>
    <%--<link type="text/css" href="../Styles/IMSOnline/SDI_Style.css" rel="Stylesheet" />


     <link rel="stylesheet nofollow" href="../Styles/IMSOnline/jquery-ui-1.9.2.custom.css" />--%>
    <script type="text/javascript" src="../js/IMSOnline/jquery-1.8.3.js"></script>
    <script type="text/javascript" src="../js/IMSOnline/jquery.bgiframe-2.1.2.js"></script>
    <script type="text/javascript" src="../js/IMSOnline/jquery-ui-1.9.2.custom.js"></script>
    <link rel="stylesheet" href="/resources/demos/style.css" />


    <style type="text/css">
        #Select1
        {
            width: 147px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <script type="text/javascript">
        function ComeBack() {
            window.close("OrderItems.aspx")
        }
    </script>
    <script type="text/javascript">

        function PassWhereClause() {



            var parentText = window.opener.document.getElementById('txtFilter');
            parentText.value = where;
            window.opener.document.getElementById('BtnGetData').click();
            window.close("FilterSearch.aspx")


        }


    </script>

    <table style="width: 100%;">
        <%--<tr>
            <td>
                <asp:Label ID="Label1" runat="server" Font-Bold="True" ForeColor="#000066" Text="Crib ID"></asp:Label>
            </td>
            <td>
                <asp:DropDownList ID="ddlGrib" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlGrib_SelectedIndexChanged"></asp:DropDownList>


                <asp:RequiredFieldValidator ForeColor="Red" Display="Dynamic" ID="rfvGrib" runat="server" InitialValue="-- SELECT --" ErrorMessage="Please Select Crib" ControlToValidate="ddlGrib" ValidationGroup="GRIBIDENT"></asp:RequiredFieldValidator>
            </td>
            <td>
                <asp:Label Visible="false" ID="lblerror" ForeColor="Red" runat="server"></asp:Label>
            </td>
        </tr>--%>
        <tr>
            <td>&nbsp;
            </td>

        </tr>
         <asp:Label Visible="false" ID="Label2" ForeColor="Red" runat="server"></asp:Label>
        <tr>
            <td>
                <asp:Label ID="lblItem" runat="server" Font-Bold="True" ForeColor="#000066" Text="Item"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblBU" runat="server" Font-Bold="True" ForeColor="#000066" Text="Business Unit"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblDecr" runat="server" Font-Bold="True" ForeColor="#000066" Text="Description"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblVendor" runat="server" Font-Bold="True" ForeColor="#000066" Text="Vendor"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblprice" runat="server" Font-Bold="True" ForeColor="#000066" Text="Price"></asp:Label>
            </td>
            <td>
                <asp:Label ID="lblqty1" runat="server" Font-Bold="True" ForeColor="#000066" Text="Quantity"></asp:Label>
            </td>
            <td>&nbsp;
            </td>
            <td>
                <asp:Label ID="lblDueDate" runat="server" Font-Bold="True" ForeColor="#000066" Text="Due Date" Visible="False"></asp:Label>
            </td>
            <td>&nbsp;
            </td>
        </tr>
        <tr>
            <td width="110">
                <asp:TextBox ID="txtItem" runat="server" AutoPostBack="true" TabIndex="1" Width="104px"></asp:TextBox>
            </td>
             <td width="110">
         
                  <asp:TextBox ID="txtGrib" runat="server" AutoPostBack="true" TabIndex="1" Enabled="False" Width="104px"></asp:TextBox>
            </td>
            <td width="210">
                <asp:TextBox ID="txtDescr" runat="server" Height="31px" TextMode="MultiLine" Width="205px"
                    Enabled="False"></asp:TextBox>
            </td>
            <td width="175">
                <asp:TextBox ID="txtVendor" runat="server" Enabled="False" Width="167px"></asp:TextBox>
            </td>
            <td width="80">
                <asp:TextBox ID="txtprice" runat="server" Enabled="False" Width="66px"></asp:TextBox>
            </td>
            <td width="85">
                <asp:TextBox ID="txtQuanity" runat="server" Width="77px" TabIndex="2"></asp:TextBox>
            </td>

            <td width="155">
                <asp:CheckBox ID="chkRushOrder" runat="server" TabIndex="4" Width="100px" Text="Rush Order" Font-Bold="true" ForeColor="Red" Font-Size="Small" />
            </td>
            <td width="155">
                <asp:TextBox ID="txtDueDate" runat="server" TabIndex="3" Width="100px" Visible="False"></asp:TextBox>
            </td>
            <td width="100">
                <%--<asp:ImageButton ID="imgAddToCart" runat="server" Height="32px" ImageUrl="~/Images/shopcartadd.png"
                        ToolTip="Add Item to Cart" Width="39px" TabIndex="4" />--%>
                <asp:Button ID="BtnAdd" runat="server" ForeColor="White" BackColor="Orange" Style="font-weight: bold" Height="20px" Width="90px" ToolTip="Add Item to Cart" Text="Add to Cart" TabIndex="4"></asp:Button>
            </td>
            <td>
                <%-- <asp:ImageButton ID="ImageButton2" runat="server" Height="32px" ImageUrl="~/Images/shopcartexclude.png"
                        Width="39px" ToolTip="Clear Item and Don't Add To Cart" CausesValidation="false" />--%>

                <asp:Button ID="BtnClear" runat="server" ForeColor="White" BackColor="Orange" Style="font-weight: bold" Height="20px" Width="90px" ToolTip="Clear Item and Don't Add To Cart" CausesValidation="false" Text="Cancel"></asp:Button>
                <%-- <asp:FileUpload ID="FileUpload1" runat="server" />--%>
            </td>
        </tr>
        <tr>
            <td>&nbsp;
            </td>
            <td>&nbsp;
            </td>
            <td>&nbsp;
            </td>
            <td>&nbsp;
            </td>
            <td>&nbsp;
            </td>
            <td>&nbsp;
            </td>
        </tr>
    </table>

    <asp:TextBox ID="txtbu" runat="server" Visible="False"></asp:TextBox>

    <%--  <table style="width: 100%;">
        <tr>
            <td>
                <br />
                <br />
                <asp:Image ID="imgCart" runat="server" Height="43px" ImageUrl="~/Images/shopcart.png"
                    Width="52px" />



                <asp:GridView ID="GridView1" runat="server" BackColor="White" BorderColor="#3366CC"
                    BorderStyle="Solid" BorderWidth="1px" CausesValidation="false" CellPadding="4"
                    EnableModelValidation="True" Height="10px" Width="959px" AutoGenerateColumns="False">
                    <Columns>
                        <asp:CommandField ShowEditButton="True"></asp:CommandField>

                        <asp:TemplateField HeaderText="Line Number" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <%#Container.DataItemIndex + 1%>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Rush Order" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:CheckBox ID="chkRush" runat="server" Text="Rush Order" Visible="false" />
                                <asp:Label ID="lblRush" runat="server" Text='<%#Eval("rush") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Unit">
                            <ItemTemplate>
                                <asp:Label ID="lblUnit" runat="server" Text='<%#Eval("Unit") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Item">
                            <ItemTemplate>
                                <asp:Label ID="lblItem" runat="server" Text='<%#Eval("Item") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Description">
                            <ItemTemplate>
                                <asp:Label ID="lblDescr" runat="server" Text='<%#Eval("Description") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Vendor">
                            <ItemTemplate>
                                <asp:Label ID="lblVendor" runat="server" Text='<%#Eval("Vendor") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Quantity">
                            <ItemTemplate>
                                <asp:TextBox ID="txtQuantity1" runat="server" Enabled="false" Text='<%#Eval("Quantity") %>'></asp:TextBox>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Cost">
                            <ItemTemplate>
                                <asp:Label ID="lblCost" runat="server" Text='<%#Eval("Cost") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Total">
                            <ItemTemplate>
                                <asp:Label ID="lblTotal" runat="server" Text='<%#Eval("Total") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <AlternatingRowStyle BackColor="#FFFF99" ForeColor="#000066" />
                    <FooterStyle BackColor="#FF9900" ForeColor="#003399" BorderStyle="Solid" />
                    <HeaderStyle BackColor="#FF9900" Font-Bold="True" ForeColor="White" />
                    <PagerStyle BackColor="#99CCCC" ForeColor="#003399" HorizontalAlign="Left" />
                    <RowStyle BackColor="White" ForeColor="#000066" />
                    <SelectedRowStyle BackColor="#003366" Font-Bold="True" ForeColor="#CCFF99" />
                </asp:GridView>
                
                <script type="text/javascript">
                    function ClientSideClick(myButton) {
                        // Client side validation
                        if (typeof (Page_ClientValidate) == 'function') {
                            if (Page_ClientValidate() == false)
                            { return false; }
                        }

                        //make sure the button is not of type "submit" but "button"
                        if (myButton.getAttribute('type') == 'button') {
                            // disable the button
                            myButton.disabled = true;
                            myButton.className = "btn-inactive";
                            myButton.value = "Processing...";
                        }
                        return true;
                    }
                </script>

                <asp:SqlDataSource ID="SqlDataSource1" runat="server"></asp:SqlDataSource>
            </td>
        </tr>
    </table>--%>

    <table style="width: 100%">
        <tr>
            <td>
                <asp:Image ID="Image1" runat="server" Height="43px" ImageUrl="~/Images/shopcart.png" Width="52px" />
                <asp:Label ID="LbleCart" runat="server" Font-Bold="True" ForeColor="#000066" Text="Items Currently in Cart"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>

                <telerik:RadGrid ID="rgIMS" runat="server" AutoGenerateColumns="false" OnNeedDataSource="rgIMS_NeedDataSource" 
                    OnUpdateCommand="rgIMS_UpdateCommand" OnItemDataBound="rgIMS_ItemDataBound" OnDeleteCommand ="rgIMS_DeleteCommand">
                    <ClientSettings EnableAlternatingItems="false">
                    </ClientSettings>
                    <MasterTableView GridLines="Both" EditMode="InPlace" DataKeyNames="Item" AutoGenerateColumns="false">
                        <Columns>
                            <telerik:GridEditCommandColumn ButtonType="ImageButton" UniqueName="EditCommandColumn">
                                <ItemStyle CssClass="MyImageButton"></ItemStyle>
                            </telerik:GridEditCommandColumn>
                            <telerik:GridTemplateColumn HeaderText="Line No" ReadOnly="true">
                                <ItemTemplate>
                                    <%#Container.DataSetIndex + 1%>
                                </ItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridTemplateColumn HeaderText="Rush Order" UniqueName="rush">
                                <ItemTemplate>
                                    <asp:Label ID="lblRushGrid" runat="server" Text='<%#Eval("rush") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:CheckBox ID="chkRush" runat="server" Text="Rush Order" />
                                    <asp:HiddenField ID="hdnRush" runat="server" Value='<%#Eval("rush") %>' />
                                </EditItemTemplate>

                            </telerik:GridTemplateColumn>

                            <telerik:GridTemplateColumn HeaderText="Unit" UniqueName="Unit">
                                <ItemTemplate>
                                    <asp:Label ID="lblitemUnit" runat="server" Text='<%#Eval("Unit")%>'></asp:Label>
                                </ItemTemplate>                               
                            </telerik:GridTemplateColumn>                         
                            <telerik:GridTemplateColumn HeaderText="Item No" UniqueName="Item">
                                <ItemTemplate>
                                    <asp:Label ID="lblitemtemp" runat="server" Text='<%#Eval("Item")%>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:Label ID="lblitemVal" runat="server" Text='<%#Eval("Item")%>' />
                                </EditItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridBoundColumn DataField="Description" HeaderText="Description" UniqueName="Description" ReadOnly="true">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="Vendor" HeaderText="Vendor" UniqueName="Vendor" ReadOnly="true">
                            </telerik:GridBoundColumn>
                            <telerik:GridTemplateColumn HeaderText="Quantity" UniqueName="Quantity">
                                <ItemTemplate>
                                    <asp:Label ID="lblQty" runat="server" Text='<%#Eval("Quantity") %>'></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtQuantity" runat="server" Text='<%#Eval("Quantity") %>'></asp:TextBox>
                                </EditItemTemplate>
                            </telerik:GridTemplateColumn>
                            <telerik:GridBoundColumn DataField="Cost" HeaderText="Cost" UniqueName="Cost" ReadOnly="true">
                            </telerik:GridBoundColumn>
                            <telerik:GridBoundColumn DataField="Total" HeaderText="Total" UniqueName="Total" ReadOnly="true">
                            </telerik:GridBoundColumn>
                            <telerik:GridButtonColumn ConfirmText="Delete this item?" ConfirmDialogType="RadWindow"
                                ConfirmTitle="Delete" ButtonType="ImageButton" CommandName="Delete" Text="Delete"
                                UniqueName="DeleteColumn">
                                <ItemStyle HorizontalAlign="Center" CssClass="MyImageButton"></ItemStyle>
                            </telerik:GridButtonColumn>
                        </Columns>
                    </MasterTableView>
                </telerik:RadGrid>
            </td>

        </tr>
        <tr>
            <td>
                <asp:Button ID="btnPlaceOrder" runat="server" Text="Place Order" BackColor="#FF9900"
                    Font-Bold="True" ForeColor="White" CausesValidation="false" OnClientClick="ClientSideClick(this)"
                    UseSubmitBehavior="False" ToolTip="Place Items In Cart On Purchase Order" />
                <asp:Label ID="lblAlert" runat="server" Font-Bold="True"></asp:Label>
            </td>  
            <td>  </td>
        </tr>
    </table>


    <script type="text/javascript">

        function cancelBack() {
            if ((event.keyCode == 8 ||
           (event.keyCode == 37 && event.altKey) ||
           (event.keyCode == 39 && event.altKey))
            &&
           (event.srcElement.form == null || event.srcElement.isTextEdit == false)
          ) {
                event.cancelBubble = true;
                event.returnValue = false;
            }
        }
    </script>
    <script type="text/javascript">
        function ClientSideClick(myButton) {
            // Client side validation
            if (typeof (Page_ClientValidate) == 'function') {
                if (Page_ClientValidate() == false)
                { return false; }
            }

            //make sure the button is not of type "submit" but "button"
            if (myButton.getAttribute('type') == 'button') {
                // disable the button
                myButton.disabled = true;
                myButton.className = "btn-inactive";
                myButton.value = "Processing...";
            }
            return true;
        }
    </script>

    <script type="text/javascript">
        function CheckNumericQty(e) {
            // Get ASCII value of key that user pressed
            var key
            if (window.event) key = window.event.keyCode;
            else if (e) key = e.which;
            else return;
            // Was key that was pressed a numeric character (0-9)?
            if (key == 46) // decimal point
                return true;
            else if (key == 8) // backspace
                return true;
            else if (key > 47 && key < 58) // digits 0-9
                return true;
            else
                return false;
        }
    </script>
</asp:Content>
