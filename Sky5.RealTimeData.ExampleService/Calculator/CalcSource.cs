using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sky5.RealTimeData.ExampleService.Calculator
{
    public class Source : DataSource
    {
        IServiceProvider ServiceProvider;

        public Source(IServiceProvider ServiceProvider)
        {
            this.ServiceProvider = ServiceProvider;
            Url = "/calc";
        }

        public override Viewport CreateViewport(QueryString query)
        {
            return new Section();
        }
    }
}
