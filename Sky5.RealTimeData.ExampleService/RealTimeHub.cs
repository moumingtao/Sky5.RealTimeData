using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.ExampleService
{
    public class RealTimeHub : DataHub
    {
        public RealTimeHub(DataSourceManager dataSourceManager) : base(dataSourceManager)
        {
        }
        public void CalcSetNum1(Guid id, int value)
        {
            var vps = (Dictionary<Guid, Viewport>)Context.Items[Util.KeyDataSections];
            var view = ((Calculator.Section)vps[id]);
            view.SetNum1(value);
        }

    }
}
