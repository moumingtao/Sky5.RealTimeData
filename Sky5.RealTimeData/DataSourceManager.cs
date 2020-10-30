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
    public class DataSourceManager<THub> : DataSourceManager where THub : Hub
    {
        public DataSourceManager(IServiceProvider provider) : base(provider)
        {
        }

        public override IHubClients Clients => provider.GetService<IHubContext<THub>>().Clients;
        public override IGroupManager Groups => provider.GetService<IHubContext<THub>>().Groups;
    }
    public abstract class DataSourceManager
    {
        protected readonly IServiceProvider provider;
        public readonly IEnumerable<DataSource> Source;
        public abstract IHubClients Clients { get; }
        public abstract IGroupManager Groups { get; }

        public DataSourceManager(IServiceProvider provider)
        {
            this.provider = provider;
            this.Source = provider.GetService<IEnumerable<DataSource>>();
            foreach (var item in Source)
            {
                item.Manager = this;
            }
        }

        public async Task<Guid> Watch(Hub hub, Uri uri)
        {
            var query = new QueryString(uri.Query);
            foreach (var item in Source)
            {
                if (!string.Equals(item.Url, uri.LocalPath, StringComparison.OrdinalIgnoreCase)) continue;
                var section = item.CreateViewport(query);
                if (section == null) continue;
                section.Source = item;
                section.AddMonitor(hub.Context);

                lock (hub.Context.Items)
                {
                    Dictionary<Guid, Viewport> secs;
                    if (hub.Context.Items.TryGetValue(Util.KeyDataSections, out var value))
                    {
                        secs = (Dictionary<Guid, Viewport>)value;
                    }
                    else
                    {
                        secs = new Dictionary<Guid, Viewport>();
                        hub.Context.Items.Add(Util.KeyDataSections, secs);
                    }
                    secs.Add(section.ID, section);
                }
                await Groups.AddToGroupAsync(hub.Context.ConnectionId, section.ID.ToString());
                if (section.CachedData != null)
                    await section.PushFullDataUseCached(hub.Clients.Client(hub.Context.ConnectionId));
                return section.ID;
            }
            return default;
        }

        public void Leave(Hub hub, Guid id)
        {
            if (hub.Context.Items.TryGetValue(Util.KeyDataSections, out var value))
            {
                var secs = (Dictionary<Guid, Viewport>)value;
                if (secs.TryGetValue(id, out var section))
                {
                    section.Remove(hub.Context);
                    secs.Remove(id);
                }
            }
        }

        public void OnDisconnectedAsync(Hub hub)
        {
            if (hub.Context.Items.TryGetValue(Util.KeyDataSections, out var value))
            {
                var secs = (Dictionary<Guid, Viewport>)value;
                foreach (var item in secs.Values)
                {
                    item.Remove(hub.Context);
                }
                secs.Clear(); 
            }
        }
    }
}
