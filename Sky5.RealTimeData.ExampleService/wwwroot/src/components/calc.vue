<template>
    <div>
        <p v-text="token"/>
        <button @click="calcSetNum1">CalcSetNum1</button>
    </div>
</template>

<script>
    import * as signalR from "@microsoft/signalr";
    export default {
        created() {
            // 配置SignalR连接
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("https://localhost:44380/RealTimeData/")
                .withAutomaticReconnect()//默认情况下，它不会自动重新连接
                .build();

            // 配置SignalR回调
            this.connection.on("patchDiff", (prevVersion, currVersion, token)=>{
                this.prevVersion = prevVersion;
                this.currVersion = currVersion;
                this.token = token;
                console.log(prevVersion, currVersion, token);
            });
            this.connection.on("pushFullData", (currVersion, token)=>{
                this.currVersion = currVersion;
                this.token = token;
                console.log(token);
            });

            this.connect();
        }, data() {
            return {
                connection: null,
                prevVersion: null,
                currVersion:null,
                token:null,
                viewportId:null,
            }
        }, computed: {
            connected() { return this.connection.connectionState == 'Connected'; }
        }, methods: {
            async connect() { // 连接SignalR服务
                await this.connection.start();
                this.viewportId = await this.connection.invoke("Watch", "/calc");
            },calcSetNum1(){
                this.connection.send("CalcSetNum1", this.viewportId, 2);
            }
        }
    }
</script>