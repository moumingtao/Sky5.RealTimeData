using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.ExampleService.Calculator
{
    public class Section : Viewport
    {
        readonly View view = new View();
        public override JToken GetRealData()
        {
            view.Sum = view.Num1 + view.Num2;
            return JToken.FromObject(view);
        }

        internal void SetNum1(int value)
        {
            view.Num1 = value;
        }
    }
}
