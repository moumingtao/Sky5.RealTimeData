using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.ExampleService.Calculator
{
    public class Source : DataSourceBase
    {
        Section Section = new Section();

        public Source(IServiceProvider ServiceProvider)
        {
            Url = "/calc";
        }

        public override ViewportBase CreateViewport(QueryString query)
        {
            return Section;
        }
    }
}
