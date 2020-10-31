using JsonDiffPatchDotNet;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sky5.RealTimeData
{
    public abstract class ViewportBase
    {
        public readonly Guid ID = Guid.NewGuid();
        public DataSourceBase DataSource { get; internal set; }
        readonly HashSet<HubCallerContext> monitors = new HashSet<HubCallerContext>();
        public JToken CachedData { get; internal set; }
        internal DateTime LastUpdateTime;
        public abstract JToken GetRealData();

        #region 失活和激活
        protected virtual void OnActivate()
        {
            CachedData = GetRealData();
            DataSource.OnActivate(this);
        }
        protected virtual void OnInactivation()
        {
            CachedData = null;
            DataSource.OnInactivation(this);
        }
        #endregion

        #region 循环刷新
        protected async ValueTask BeginLoop(int DelayMilliseconds)
        {
            do
            {
                var begin = DateTime.Now;
                try
                {
                    await ExecuteWorkeOnce();
                    var delay = DelayMilliseconds - (int)(DateTime.Now - begin).TotalMilliseconds;
                    if (delay > 0)
                    {
                        if (delay > DelayMilliseconds)
                            await Task.Delay(DelayMilliseconds);
                        else
                            await Task.Delay(delay);
                    }
                }
                catch (Exception ex)
                {
                    await OnLoopWorkerException(ex);
                }
            } while (monitors.Count > 0);
        }
        public abstract ValueTask ExecuteWorkeOnce();
        protected virtual Task OnLoopWorkerException(Exception ex) => Task.Delay(5000);
        #endregion

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

        #region 限制频率的JsonDiffPatch
        int invalid;
        ValueTask LastPushTask;
        /// <summary>
        /// 通知该视图有变更，内部会每隔100ms向客户端推送变更，直到100ms内没有通知变更，该方法非常高效可频繁调用
        /// </summary>
        /// <returns>值类型Task，用于等待推送完成</returns>
        public ValueTask NotifyChanged()
        {
            if (Interlocked.Exchange(ref invalid, 2) == 0)
                LastPushTask = LoopPushDiff();
            return LastPushTask;
        }

        JsonDiffPatch jdp = new JsonDiffPatch();
        async ValueTask LoopPushDiff()
        {
            do
            {
                while (Interlocked.CompareExchange(ref invalid, 1, 2) == 2)
                {
                    try
                    {
                        var begin = DateTime.Now;
                        var realData = GetRealData();
                        var client = DataSource.Manager.Clients.Group(ID.ToString());
                        if (CachedData == null)
                        {
                            CachedData = realData;
                            LastUpdateTime = DateTime.Now;
                            await PushFullDataUseCached(client);
                        }
                        else
                        {
                            var token = jdp.Diff(CachedData, realData);
                            if (token != null)
                            {
                                CachedData = realData;
                                var prevTime = LastUpdateTime;
                                LastUpdateTime = DateTime.Now;
                                await client.SendCoreAsync("PatchDiff", new object[] { ID, prevTime, LastUpdateTime, token });
                            }
                        }
                        var delay = 100 - (int)(DateTime.Now - begin).TotalMilliseconds;
                        if (delay > 0 && delay < 1000) await Task.Delay(delay);
                    }
                    catch (Exception)
                    {
                        await Task.Delay(3000);
                    }
                }
            } while (Interlocked.CompareExchange(ref invalid, 0, 1) == 2);
        }
        #endregion

        internal Task PushFullDataUseCached(IClientProxy client) => client.SendCoreAsync("PushFullData", new object[] { ID, LastUpdateTime, CachedData });
    }
}
