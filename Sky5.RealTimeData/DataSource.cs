using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sky5.RealTimeData
{
    /// <summary>
    /// 封装一个数据源
    /// </summary>
    public abstract class DataSource
    {
        public virtual DataSourceManager Manager { get; internal set; }

        public string Url;

        /// <summary>
        /// 尝试获取一个视图，一个数据源可以有多个视图
        /// </summary>
        /// <param name="uri">用于获取视图的Url</param>
        /// <returns>uri匹配成功返回视图对象，否则返回null</returns>
        public abstract Viewport CreateViewport(QueryString query);

        public virtual Task HttpHandle(HttpContext context)
        {
            var view = CreateViewport(context.Request.QueryString);
            if (view == null)
                return context.Response.WriteAsync($"the query string err:" + context.Request.QueryString);
            else
            {
                context.Response.ContentType = "application/json;charset=UTF-8";
                return context.Response.WriteAsync(view.GetRealData().ToString());
            }
        }
    }
}
