using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sky5.RealTimeData
{
    public class DataHub : Hub
    {
        readonly DataSourceManager DataSourceManager;
        public DataHub(DataSourceManager dataSourceManager)
        {
            DataSourceManager = dataSourceManager;
        }
        public async Task<Guid> Watch(string url)
        {
            //return default;
            return await DataSourceManager.Watch(this, url);
        }

        public void Leave(Guid id)
        {
            //return;
            DataSourceManager.Leave(this, id);
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            // return Task.CompletedTask;
            DataSourceManager.OnDisconnectedAsync(this);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
