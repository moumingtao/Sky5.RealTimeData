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
        public void CalcSetNum1(Guid id, int value) => ((Calculator.Section)Viewports[id]).SetNum1(value);

    }
}
