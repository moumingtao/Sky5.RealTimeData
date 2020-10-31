import * as signalR from "@microsoft/signalr";
import * as jsondiffpatch from 'jsondiffpatch'
export function Viewport(url, hub){
    this.url = url;
    this.id = null;
    this.hub = hub;
}
Viewport.prototype.watch = async function(){
    var state = await this.hub.connection.invoke("Watch", this.url);
    if(state){
        this.id = state.id;
        this.currVersion = state.lastUpdateTime;
        this.data = state.data;
    }
}
export function DataHub(hubUrl){
    this.viewports = [];
    // 配置SignalR连接
    this.connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()//默认情况下，它不会自动重新连接
        .build();
    this.connection.on("PushFullData", this.PushFullData.bind(this));
    this.connection.on("PatchDiff", this.PatchDiff.bind(this));
}
DataHub.prototype.getViewport = async function(url){
    for (const key in this.viewports) {
        if(this.viewports[key].url == url) return this.viewports[key];
    }
    var vp = new Viewport(url, this);
    if(this.connection.connectionState != "Connected")
        await this.connection.start();
    await vp.watch();
    this.viewports[vp.id] = vp;
    return vp;
}

// 拉取到整个数据
DataHub.prototype.PushFullData = function(vpId, currVersion, fullData){
    var vp = this.viewports[vpId];
    vp.currVersion = currVersion;
    vp.data = fullData;
}
// 拉取到差异数据
DataHub.prototype.PatchDiff = function(vpId, prevVersion, currVersion, diff){
    var vp = this.viewports[vpId];
    if(vp.currVersion != prevVersion)
        vp.watch();
    else{
        vp.currVersion = currVersion;
        jsondiffpatch.patch(vp.data, diff)
    }
}

var hub = new DataHub("https://localhost:44380/RealTimeData/");
export {hub};
