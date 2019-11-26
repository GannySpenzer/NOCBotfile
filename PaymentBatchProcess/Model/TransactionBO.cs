using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanquestUtility.Model
{
    public class TransactionBO
    {
        public int id { get; set; }
        public DateTime created_at { get; set; }
        public CardDetails card_details { get; set; }
        public AmountDetails amount_details { get; set; }
        public TransactionDetails transaction_details { get; set; }
        public Customer customer { get; set; }
        public StatusDetails status_details { get; set; }
        public BillingInfo billing_info { get; set; }
        public ShippingInfo shipping_info { get; set; }
    }
    public class CardDetails
    {
        public object name { get; set; }
        public string last4 { get; set; }
        public int expiry_month { get; set; }
        public int expiry_year { get; set; }
        public string card_type { get; set; }
        public object avs_street { get; set; }
        public object avs_zip { get; set; }
        public string auth_code { get; set; }
        public string avs_result { get; set; }
        public string cvv_result { get; set; }
    }

    public class AmountDetails
    {
        public string amount { get; set; }
        public string original_requested_amount { get; set; }
        public string original_authorized_amount { get; set; }
        public string tax { get; set; }
        public string surcharge { get; set; }
        public string shipping { get; set; }
        public string tip { get; set; }
        public string discount { get; set; }
        public string subtotal { get; set; }
    }

    public class TransactionDetails
    {
        public int batch_id { get; set; }
        public object description { get; set; }
        public object clerk { get; set; }
        public object terminal { get; set; }
        public object client_ip { get; set; }
        public string invoice_number { get; set; }
        public object po_number { get; set; }
        public object signature { get; set; }
        public string source { get; set; }
        public object username { get; set; }
        public string type { get; set; }
        public object reference_number { get; set; }
    }

    public class Customer
    {
        public object identifier { get; set; }
        public string email { get; set; }
        public object fax { get; set; }
        public object customer_id { get; set; }
    }

    public class StatusDetails
    {
        public string status { get; set; }
    }

    public class BillingInfo
    {
        public object first_name { get; set; }
        public object last_name { get; set; }
        public object street { get; set; }
        public object street2 { get; set; }
        public object city { get; set; }
        public object state { get; set; }
        public object zip { get; set; }
        public object country { get; set; }
        public object phone { get; set; }
    }

    public class ShippingInfo
    {
        public object first_name { get; set; }
        public object last_name { get; set; }
        public object street { get; set; }
        public object street2 { get; set; }
        public object city { get; set; }
        public object state { get; set; }
        public object zip { get; set; }
        public object country { get; set; }
        public object phone { get; set; }
    }
}
