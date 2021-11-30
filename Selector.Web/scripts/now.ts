import * as signalR from "@microsoft/signalr";
import * as Vue from "vue";
import { TrackAudioFeatures, PlayCount, CurrentlyPlayingDTO } from "./HubInterfaces";
import NowPlayingCard from "./Now/NowPlayingCard";
import { AudioFeatureCard, AudioFeatureChartCard, PopularityCard, SpotifyLogoLink } from "./Now/Spotify";
import { PlayCountCard } from "./Now/LastFm";
import BaseInfoCard from "./Now/BaseInfoCard";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub")
    .build();

connection.start()
.then(val => {
    connection.invoke("OnConnected");
})
.catch(err => console.error(err));

interface InfoCard {
    html: string
}

interface NowPlaying {
    currentlyPlaying?: CurrentlyPlayingDTO,
    trackFeatures?: TrackAudioFeatures,
    playCount?: PlayCount,
    cards: InfoCard[]
}

const app = Vue.createApp({
    data() {
        return {
            currentlyPlaying: undefined,
            trackFeatures: undefined,
            playCount: undefined,
            cards: []
        } as NowPlaying
    },
    created() {
        connection.on("OnNewPlaying", (context: CurrentlyPlayingDTO) => 
        {
            console.log(context);
            this.currentlyPlaying = context;
            this.trackFeatures = null;
            this.playCount = null;
            this.cards = [];

            if(context.track !== null && context.track !== undefined)
            {
                connection.invoke("SendAudioFeatures", context.track.id);
                connection.invoke("SendPlayCount",
                    context.track.name,
                    context.track.artists[0].name,
                    context.track.album.name,
                    context.track.album.artists[0].name
                );
            }
        });

        connection.on("OnNewAudioFeature", (feature: TrackAudioFeatures) => 
        {
            console.log(feature);
            this.trackFeatures = feature;
        });

        connection.on("OnNewPlayCount", (count: PlayCount) => {

            console.log(count);
            this.playCount = count;
        });
    }
});

app.component("now-playing-card", NowPlayingCard);
app.component("audio-feature-card", AudioFeatureCard);
app.component("audio-feature-chart-card", AudioFeatureChartCard);
app.component("info-card", BaseInfoCard);
app.component("popularity", PopularityCard);
app.component("spotify-logo", SpotifyLogoLink);
app.component("play-count-card", PlayCountCard);
const vm = app.mount('#app');