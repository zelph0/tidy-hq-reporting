using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TidyHQ.Resources
{
    class GroupStructure
    {
        public string id { get; set; }
        public string label { get; set; }

        public string description { get; set; }
        public string created_at { get; set; }
        public string contacts_count { get; set; }
        public string logo_image { get; set; }
    }
}
