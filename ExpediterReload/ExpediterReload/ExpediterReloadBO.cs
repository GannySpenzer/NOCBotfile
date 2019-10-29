using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpediterReload
{
    class ExpediterReloadBO
    {

        public string Business_Unit  { get; set; }
        public string Buyer_ID { get; set; }
        public string Buyer_Team { get; set; }
        public string Client { get; set; }
        public string Description { get; set; }
        public string Expediting_Comments { get; set; }
        public string Inventory_Business_Unit { get; set; }
        public string Item { get; set; }
        public string Last_Comment_Date { get; set; }
        public string Last_Operator { get; set; }
        public string Line_Number { get; set; }
        public string PO_Date { get; set; }
        public string PO_ID { get; set; }
        public string PS_URL { get; set; }
        public string Priority_Flag { get; set; }
        public string Problem_Code { get; set; }
        public string Site_Name { get; set; }
        public string Status_Age { get; set; }
        public string Vendor_ID { get; set; }
        public string Vendor_Name { get; set; }

    }

    public class Item
    {
        public string tableName { get; set; }
        public int count { get; set; }
        public List<string> columnNames { get; set; }
        public List<List<string>> rows { get; set; }
    }

    public class Link
    {
        public string rel { get; set; }
        public string href { get; set; }
        public string mediaType { get; set; }
    }

    public class RootObject
    {
        public List<Item> items { get; set; }
        public List<Link> links { get; set; }
    }

}
