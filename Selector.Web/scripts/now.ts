import * as signalR from "@microsoft/signalr";
import * as Vue from "vue";
import { TrackAudioFeatures, PlayCount, CurrentlyPlayingDTO, CountSample } from "./HubInterfaces";
import NowPlayingCard from "./Now/NowPlayingCard";
import { AudioFeatureCard, AudioFeatureChartCard, PopularityCard, SpotifyLogoLink } from "./Now/Spotify";
import { PlayCountChartCard, CombinedPlayCountChartCard } from "./Now/PlayCountGraph";
import { ArtistBreakdownChartCard } from "./Now/ArtistBreakdownGraph";
import { PlayCountCard, LastFmLogoLink } from "./Now/LastFm";
import BaseInfoCard from "./Now/BaseInfoCard";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/nowhub")
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

export interface ScrobbleDataSeries {
    label: string,
    colour: string,
    data: CountSample[]
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
        showArtistChart(){
            return this.playCount !== null && this.playCount !== undefined && this.playCount.artistCountData.length > 3;
        },
        showAlbumChart() {
            return this.playCount !== null && this.playCount !== undefined && this.playCount.albumCountData.length > 3;
        },
        showTrackChart(){
            return this.playCount !== null && this.playCount !== undefined && this.playCount.trackCountData.length > 3;
        },
        showArtistBreakdown(){
            return this.playCount !== null && this.playCount !== undefined && this.playCount.artist > 0;
        },
        earliestDate(){
            return this.playCount.artistCountData[0].timeStamp;
        },
        latestDate(){
            return this.playCount.artistCountData.at(-1).timeStamp;
        },
        trackGraphTitle() { return `${this.currentlyPlaying.track.name} 🎵`},
        albumGraphTitle() { return `${this.currentlyPlaying.track.album.name} 💿`},
        artistGraphTitle() { return `${this.currentlyPlaying.track.artists[0].name} 🎤`},
        combinedData(){
            return [
            { 
                label: "artist",
                colour: "#598556",
                data: this.playCount.artistCountData
            }, 
            { 
                label: "album",
                colour: "#a34c77",
                data: this.playCount.albumCountData
            },
            { 
                label: "track",
                colour: "#7a99c2",
                data: this.playCount.trackCountData
            }];
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
app.component("play-count-chart-card-comb", CombinedPlayCountChartCard);
app.component("artist-breakdown", ArtistBreakdownChartCard);
const vm = app.mount('#app');