﻿<%@ Page Language="VB" AutoEventWireup="false" CodeFile="payment_form.aspx.vb" Inherits="payment_form" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head>
    <title>Secure Acceptance - Payment Form Example</title>
    <link rel="stylesheet" type="text/css" href="Styles/payment.css"/>
    <script type="text/javascript" src="Scripts/jquery-1.7.min.js"></script>
</head>
<body>
<form id="payment_form" action="payment_confirmation.aspx" method="post">
    <input type="hidden" name="access_key" value="41ba658448c63874bbe636ddd2ea8070"/>
    <input type="hidden" name="profile_id" value="5F67876F-6F9B-4158-887E-88A1EF0EC0E9"/>
    <input type="hidden" name="transaction_uuid" value="<% Response.Write(getUUID()) %>"/>
    <input type="hidden" name="signed_field_names" value="access_key,profile_id,transaction_uuid,signed_field_names,unsigned_field_names,signed_date_time,locale,transaction_type,reference_number,amount,currency"/>
    <input type="hidden" name="unsigned_field_names"/>
    <input type="hidden" name="signed_date_time" value="<% Response.Write(getUTCDateTime()) %>"/>
    <input type="hidden" name="locale" value="en"/>
    <fieldset>
        <legend>Payment Details</legend>
        <div id="paymentDetailsSection" class="section">
            <span>transaction_type:</span><input type="text" name="transaction_type" size="25"/><br/>
            <span>reference_number:</span><input type="text" name="reference_number" size="25"/><br/>
            <span>amount:</span><input type="text" name="amount" size="25"/><br/>
            <span>currency:</span><input type="text" name="currency" size="25"/><br/>
        </div>
    </fieldset>
    <input type="submit" id="submit" name="submit" value="Submit"/>
    <script type="text/javascript" src="Scripts/payment_form.js"></script>
</form>
</body>
</html>
