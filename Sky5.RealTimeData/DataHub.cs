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
        public Task<Guid> Watch(Uri uri) => DataSourceManager.Watch(this, uri);
        public void Leave(Guid id) => DataSourceManager.Leave(this, id);
        public override Task OnDisconnectedAsync(Exception exception)
        {
            DataSourceManager.OnDisconnectedAsync(this);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
