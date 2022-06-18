import * as signalR from "@microsoft/signalr";
import * as Vue from "vue";
import { TrackAudioFeatures, PlayCount, CurrentlyPlayingDTO } from "./HubInterfaces";
import NowPlayingCard from "./Now/NowPlayingCard";
import { AudioFeatureCard, AudioFeatureChartCard, PopularityCard, SpotifyLogoLink } from "./Now/Spotify";
import { PlayCountChartCard } from "./Now/PlayCountGraph";
import { PlayCountCard, LastFmLogoLink } from "./Now/LastFm";
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
    Content: string
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
    computed: {
        lastfmTrack() {
            return {
                name: this.currentlyPlaying.track.name,
                artist: this.currentlyPlaying.track.artists[0].name,
                album: this.currentlyPlaying.track.album.name,
                album_artist: this.currentlyPlaying.track.album.artists[0].name,
            };
        },
        lastfmArtist(){

            // if(this.currentlyPlaying.track.artists[0].length > 0)
            {
                return this.currentlyPlaying.track.artists[0].name;
            }
            return "";
        },
        showArtistChart(){
            return this.playCount !== null && this.playCount !== undefined && this.playCount.artistCountData.length > 0;
        },
        showAlbumChart() {
            return this.playCount !== null && this.playCount !== undefined && this.playCount.albumCountData.length > 0;
        },
        showTrackChart(){
            return this.playCount !== null && this.playCount !== undefined && this.playCount.trackCountData.length > 0;
        }
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
                if(context.track.id !== null)
                {
                    connection.invoke("SendAudioFeatures", context.track.id);
                }
                connection.invoke("SendPlayCount",
                    context.track.name,
                    context.track.artists[0].name,
                    context.track.album.name,
                    context.track.album.artists[0].name
                );
                connection.invoke("SendFacts",
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

        connection.on("OnNewCard", (card: InfoCard) => {

            console.log(card);
            this.cards.push(card);
        });
    }
});

app.component("now-playing-card", NowPlayingCard);
app.component("audio-feature-card", AudioFeatureCard);
app.component("audio-feature-chart-card", AudioFeatureChartCard);
app.component("info-card", BaseInfoCard);
app.component("popularity", PopularityCard);
app.component("spotify-logo", SpotifyLogoLink);
app.component("lastfm-logo", LastFmLogoLink);
app.component("play-count-card", PlayCountCard);
app.component("play-count-chart-card", PlayCountChartCard);
const vm = app.mount('#app');