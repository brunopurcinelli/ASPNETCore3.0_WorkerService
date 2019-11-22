using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceSiteHost.Models
{
    public class ResultMonitor
    {
        public string Hour { get; set; }
        public string Host { get; set; }
        public string Status { get; set; }
        public object Exception { get; set; }
    }
}
