using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCSendNotes.BO
{
    public class SendNotesBO
    {
        public string Note { get; set; }
        public string MailedTo { get; set; }
        public bool ActionRequired { get; set; }
        public int Visibility { get; set; }
        public string Actor { get; set; }
        public bool DoNotSendEmail { get; set; }
        public bool NotifyFollowers { get; set; }
        public DateTime ScheduledDate { get; set; }
    }
}
