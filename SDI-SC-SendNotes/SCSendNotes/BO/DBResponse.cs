using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCSendNotes.BO
{
    public class DBResponse
    {
        public string Order { get; set; }
        public int ThirdPartyCompID { get; set; }
        public int Line { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public string DeliveryDate { get; set; }
        public int Quantity { get; set; }
        public string WorkOrder { get; set; }
        public string Email { get; set; }
        public string TrackningNo { get; set; }
        public string Url { get; set; }
        public string LinePartStatus { get; set; }
        public string DeliveryLocation { get; set; }
        public object PartImage { get; set; }
        public string OrderDate { get; set; }
    }
}
