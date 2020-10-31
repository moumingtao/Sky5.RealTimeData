using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sky5.RealTimeData
{
    class DataSourceManager<THub> : DataSourceManager where THub : Hub
    {
        public DataSourceManager(IServiceProvider provider) : base(provider) { }
        public override IHubClients Clients => provider.GetService<IHubContext<THub>>().Clients;
        public override IGroupManager Groups => provider.GetService<IHubContext<THub>>().Groups;
    }
    public abstract class DataSourceManager
    {
        protected readonly IServiceProvider provider;
        public readonly Dictionary<string, DataSource> Source = new Dictionary<string, DataSource>(StringComparer.OrdinalIgnoreCase);
        public abstract IHubClients Clients { get; }
        public abstract IGroupManager Groups { get; }

        public DataSourceManager(IServiceProvider provider)
        {
            this.provider = provider;
            foreach (var item in provider.GetService<IEnumerable<DataSource>>())
            {
                Source.Add(item.Url, item);
                item.Manager = this;
            }
        }

        public async Task<ViewportState> Watch(Hub hub, string url)
        {
            var info = url.Split('?', 2);
            if(Source.TryGetValue(info[0], out var item))
            {
                var viewport = item.CreateViewport(info.Length > 1 ? new QueryString(info[1]) : default);
                if (viewport == null) return default;
                viewport.Source = item;
                viewport.AddMonitor(hub.Context);

                lock (hub.Context.Items)
                {
                    Dictionary<Guid, Viewport> secs;
                    if (hub.Context.Items.TryGetValue(Util.KeyDataSections, out var value))
                        secs = (Dictionary<Guid, Viewport>)value;
                    else
                    {
                        secs = new Dictionary<Guid, Viewport>();
                        hub.Context.Items.Add(Util.KeyDataSections, secs);
                    }
                    secs.Add(viewport.ID, viewport);
                }
                await Groups.AddToGroupAsync(hub.Context.ConnectionId, viewport.ID.ToString());
                return new ViewportState { Id = viewport.ID, Data = viewport.CachedData, LastUpdateTime = viewport.LastUpdateTime };
            }
            return default;
        }

        public async Task Leave(Hub hub, Guid id)
        {
            if (hub.Context.Items.TryGetValue(Util.KeyDataSections, out var value))
            {
                var vps = (Dictionary<Guid, Viewport>)value;
                if (vps.TryGetValue(id, out var viewport))
                {
                    vps.Remove(id);
                    viewport.Remove(hub.Context);
                    await Groups.RemoveFromGroupAsync(hub.Context.ConnectionId, viewport.ID.ToString());
                }
            }
        }

        public async Task OnDisconnectedAsync(Hub hub)
        {
            if (hub.Context.Items.TryGetValue(Util.KeyDataSections, out var value))
            {
                var secs = (Dictionary<Guid, Viewport>)value;
                foreach (var viewport in secs.Values)
                {
                    viewport.Remove(hub.Context);
                    await Groups.RemoveFromGroupAsync(hub.Context.ConnectionId, viewport.ID.ToString());
                }
                secs.Clear(); 
            }
        }
    }
}
