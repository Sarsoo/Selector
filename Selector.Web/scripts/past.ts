import * as signalR from "@microsoft/signalr";
// import { stringifyStyle } from "@vue/shared";
import * as Vue from "vue";
import { RankResult, RankEntry, PastParams } from "./HubInterfaces";
import { RankCard } from "./Past/RankCard";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/pasthub")
    .build();

connection.start()
.then(val => {
    connection.invoke("OnConnected");
})
.catch(err => console.error(err));

const app = Vue.createApp({
    data() {
        return {
            track: "",
            album: "",
            artist: "",

            from: null,
            to: null,

            trackEntries: [],
            albumEntries: [],
            artistEntries: [],
        }
    },
    created() {
        connection.on("OnRankResult", (result: RankResult) => 
        {
            console.log(result);

            this.trackEntries = result.trackEntries;
            this.albumEntries = result.albumEntries;
            this.artistEntries = result.artistEntries;
        });
    },
    methods: {
        submit() {
            console.log({
                "track": this.track,
                "album": this.album,
                "artist": this.artist,
                "from": this.from,
                "to": this.to,
            });

            connection.invoke("OnSubmitted", {
                track: this.track,
                album: this.album,
                artist: this.artist,
                from: this.from,
                to: this.to,
            } as PastParams);
        }
    }
});

app.component("rank-card", RankCard);

const vm = app.mount('#pastapp');