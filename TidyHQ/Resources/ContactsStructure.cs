using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TidyHQ.Resources
{
    public class ContactsStructure
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string nick_name { get; set; }
        public string company { get; set; }
        public string email_address { get; set; }
        public string phone_number { get; set; }
        public string address1 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string postcode { get; set; }
        public string gender { get; set; }
        public string birthday { get; set; }
        public string facebook { get; set; }
        public string twitter { get; set; }
        public string details { get; set; }
        public string subscribed { get; set; }
        public string metadata { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string emergency_contact_person { get; set; }
        public string emergency_contact_number { get; set; }
        public string profile_image { get; set; }
        public string status { get; set; }

    }
}
