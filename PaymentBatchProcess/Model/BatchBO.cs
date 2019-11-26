using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanquestUtility.Model
{
    class BatchBO
    {
        public int id { get; set; }
        public DateTime opened_at { get; set; }
        public object auto_close_date { get; set; }
        public DateTime closed_at { get; set; }
        public string platform { get; set; }
        public int sequence_number { get; set; }
        public double charges_sum { get; set; }
        public int charges_count { get; set; }
        public int credits_sum { get; set; }
        public int credits_count { get; set; }
        public int transactions_count { get; set; }
    }
}
