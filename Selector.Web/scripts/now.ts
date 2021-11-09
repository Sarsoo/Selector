import * as signalR from "@microsoft/signalr";
import * as Vue from "vue";
import { FullTrack, CurrentlyPlayingDTO } from "./HubInterfaces";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub")
    .build();

connection.on("OnNewPlaying", (context: CurrentlyPlayingDTO) => console.log(context));

connection.start().catch(err => console.error(err));

const app = Vue.createApp({
    data() {
        return {
            count: 4
        }
    },
    created() {
        console.log(this.count);
    }
});
// app.component("", TrackNowPlaying);
const vm = app.mount('#app');