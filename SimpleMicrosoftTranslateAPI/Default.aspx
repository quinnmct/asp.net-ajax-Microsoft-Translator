<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
	<title></title>
	<telerik:RadStyleSheetManager id="RadStyleSheetManager1" runat="server" />
</head>
<body>
    <form id="form1" runat="server">
	<telerik:RadScriptManager ID="RadScriptManager1" runat="server">
		<Scripts>
			<asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.Core.js" />
			<asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.jQuery.js" />
			<asp:ScriptReference Assembly="Telerik.Web.UI" Name="Telerik.Web.UI.Common.jQueryInclude.js" />
		</Scripts>
	</telerik:RadScriptManager>
	<script type="text/javascript">
	    var selectedLang = "es";

	    function mycallback(response) {
	        alert(response);
	    }

	    function translateTime() {
	        var from = "en";
	        var to = selectedLang;
	        var text = $('#RadTextBox1').val();

	        var s = document.createElement("script");
	        s.src = "http://api.microsofttranslator.com/V2/Ajax.svc/Translate" +
                "?appId=Bearer " + encodeURIComponent(window.accessToken) +
                "&from=" + encodeURIComponent(from) +
                "&to=" + encodeURIComponent(to) +
                "&text=" + encodeURIComponent(text) +
                "&oncomplete=mycallback";
	        document.body.appendChild(s);
	    }

	    //CANNOT figure out how to select the value of the selected RadComboBoxItem
        //ended up setting the text of each item to the actual language code.
	    function OnClientSelectedIndexChanged(sender, eventArgs) {
	        var item = eventArgs.get_item();
	        selectedLang = item.get_text();  //item.get_value() ?  item.val() ?
	        alert(selectedLang);
	    }
    </script>
	<telerik:RadAjaxManager ID="RadAjaxManager1" runat="server">
	</telerik:RadAjaxManager>
	<div>
        <telerik:RadTextBox ID="RadTextBox1" runat="server"></telerik:RadTextBox><br />
        <telerik:RadComboBox ID="langChoice" runat="server" OnClientSelectedIndexChanged="OnClientSelectedIndexChanged">
            <Items>                                            
                <telerik:RadComboBoxItem runat="server" Text="es" Value="es"></telerik:RadComboBoxItem>
                <telerik:RadComboBoxItem runat="server" Text="fr" Value="fr"></telerik:RadComboBoxItem>
                <telerik:RadComboBoxItem runat="server" Text="de" Value="de"></telerik:RadComboBoxItem>
                <telerik:RadComboBoxItem runat="server" Text="ru" Value="ru"></telerik:RadComboBoxItem>
                <telerik:RadComboBoxItem runat="server" Text="ja" Value="ja"></telerik:RadComboBoxItem>
                <telerik:RadComboBoxItem runat="server" Text="sv" Value="sv"></telerik:RadComboBoxItem>
            </Items>
        </telerik:RadComboBox><input type="button" onclick="translateTime();" value="click me" />
        <div id="outputDiv"></div>
	</div>
	</form>
</body>
</html>
