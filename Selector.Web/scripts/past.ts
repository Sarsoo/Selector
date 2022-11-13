import * as signalR from "@microsoft/signalr";
// import { stringifyStyle } from "@vue/shared";
import * as Vue from "vue";
import { RankResult, RankEntry, PastParams } from "./HubInterfaces";
import { RankCard } from "./Past/RankCard";
import { CountCard } from "./Past/CountCard";
import { PlayCountChartCard } from "./Now/PlayCountGraph";
import { LastFmLogoLink } from "./Now/LastFm";

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

            resampledSeries: [],

            totalCount: 0
        }
    },
    created() {
        connection.on("OnRankResult", (result: RankResult) => 
        {
            console.log(result);

            this.trackEntries = result.trackEntries;
            this.albumEntries = result.albumEntries;
            this.artistEntries = result.artistEntries;
            this.resampledSeries = result.resampledSeries;
            this.totalCount = result.totalCount;
        });
    },
    methods: {
        submit() {
            let context = {
                track: this.track,
                album: this.album,
                artist: this.artist,
                from: this.from,
                to: this.to,
            } as PastParams;

            console.log(context);

            connection.invoke("OnSubmitted", context);
        }
    }
});

app.component("play-count-chart-card", PlayCountChartCard);
app.component("rank-card", RankCard);
app.component("lastfm-logo", LastFmLogoLink);
app.component("count-card", CountCard);

const vm = app.mount('#pastapp');