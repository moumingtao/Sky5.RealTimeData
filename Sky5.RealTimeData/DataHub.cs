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

        static Dictionary<Guid, ViewportBase> EmptyViewports = new Dictionary<Guid, ViewportBase>();
        protected Dictionary<Guid, ViewportBase> Viewports
        {
            get
            {
                if (Context.Items.TryGetValue(Util.KeyDataSections, out var value))
                    return (Dictionary<Guid, ViewportBase>)value;
                else
                    return EmptyViewports;
            }
        }

        /// <summary>
        /// 开始监听指定数据源
        /// </summary>
        /// <param name="url">指定数据源的url</param>
        /// <returns>被监听数据窗口的id</returns>
        public Task<ViewportState> Watch(string url) => DataSourceManager.Watch(this, url);

        /// <summary>
        /// 取消监听指定数据源
        /// </summary>
        /// <param name="id">指定数据源窗口的id</param>
        public Task Leave(Guid id) => DataSourceManager.Leave(this, id);

        public override Task OnDisconnectedAsync(Exception exception) => DataSourceManager.OnDisconnectedAsync(this);
    }
}
