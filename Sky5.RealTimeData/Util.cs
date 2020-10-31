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
            services.AddSingleton<IEnumerable<DataSourceBase>>(provider => {
                var ss = new List<DataSourceBase>();
                foreach (var item in services)
                {
                    if (typeof(DataSourceBase).IsAssignableFrom(item.ServiceType))
                    {
                        var s = (DataSourceBase)provider.GetService(item.ServiceType);
                        ss.Add(s);
                    }
                }
                return ss;
            });
            services.AddSingleton<DataSourceManager, DataSourceManager<THub>>();
        }
    }
}
