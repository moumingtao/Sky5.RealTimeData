using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sky5.RealTimeData
{
    public static class Util
    {
        public const string KeyDataSections = "Sky5.RealTimeData.DataSections";
        public static void AddRealTimeManager<THub>(this IServiceCollection services) where THub : Hub
        {
            services.AddSingleton<IEnumerable<DataSource>>(provider => {
                var ss = new List<DataSource>();
                foreach (var item in services)
                {
                    if (typeof(DataSource).IsAssignableFrom(item.ServiceType))
                    {
                        var s = (DataSource)provider.GetService(item.ServiceType);
                        ss.Add(s);
                    }
                }
                return ss;
            });
            services.AddSingleton<DataSourceManager, DataSourceManager<THub>>();
        }
    }
}
