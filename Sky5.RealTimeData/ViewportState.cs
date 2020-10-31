using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sky5.RealTimeData
{
    public class ViewportState
    {
        public Guid Id { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public JToken Data { get; set; }
    }
}
