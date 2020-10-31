<template>
    <div>
        <p v-text="viewport.data"/>
        <button @click="calcSetNum1">CalcSetNum1</button>
    </div>
</template>

<script>
import * as realtime from '@/utils/realtimedata.js'
export default {
    async created(){
        this.viewport = await realtime.hub.getViewport('/calc');
    }, data() {
        return { viewport:{} }
    }, methods: {
        calcSetNum1(){
            realtime.hub.connection.send("CalcSetNum1", this.viewport.id, this.viewport.data.Num1 + 1);
        }
    }
}
</script>