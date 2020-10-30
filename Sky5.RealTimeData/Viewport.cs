using JsonDiffPatchDotNet;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sky5.RealTimeData
{
    public abstract class Viewport
    {
        public readonly Guid ID = Guid.NewGuid();
        public DataSource Source { get; internal set; }
        readonly HashSet<HubCallerContext> monitors = new HashSet<HubCallerContext>();
        JsonDiffPatch jdp = new JsonDiffPatch();
        public JToken CachedData;
        DateTime LastUpdateTime;
        public abstract JToken GetRealData();

        internal void AddMonitor(HubCallerContext monitor)
        {
            bool leaveEmpty;
            lock (monitors)
            {
                if (!monitors.Add(monitor)) return;
                leaveEmpty = monitors.Count == 1;
            }
            if (leaveEmpty) OnActivate();
        }

        protected virtual void OnActivate() { }
        protected virtual void OnInactivation() { }

        internal void Remove(HubCallerContext monitor)
        {
            bool toEmpty;
            lock (monitors)
            {
                if (!monitors.Remove(monitor)) return;
                toEmpty = monitors.Count == 0;
            }
            if (toEmpty) OnInactivation();
        }
        internal Task PushDiff(Hub hub)
        {
            var realData = GetRealData();
            var client = hub.Clients.Group(ID.ToString());
            if (CachedData == null)
            {
                CachedData = realData;
                LastUpdateTime = DateTime.Now;
                return  PushFullDataUseCached(client);
            }
            else
            {
                var token = jdp.Patch(CachedData, realData);
                CachedData = realData;
                var prevTime = LastUpdateTime;
                LastUpdateTime = DateTime.Now;
                return client.SendCoreAsync("PatchDiff", new object[] { prevTime, LastUpdateTime, token });
            }
        }

        internal Task PushFullDataUseCached(IClientProxy client) => client.SendCoreAsync("PushFullData", new object[] { LastUpdateTime, CachedData });
    }
}
