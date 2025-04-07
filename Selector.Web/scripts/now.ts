import * as signalR from "@microsoft/signalr";
import * as Vue from "vue";
import {
    AppleCurrentlyPlayingDTO,
    CountSample,
    PlayCount,
    SpotifyCurrentlyPlayingDTO,
    TrackAudioFeatures
} from "./HubInterfaces";
import NowPlayingCardSpotify from "./Now/NowPlayingCardSpotify";
import NowPlayingCardApple from "./Now/NowPlayingCardApple";
import {AudioFeatureCard, AudioFeatureChartCard, PopularityCard, SpotifyLogoLink} from "./Now/Spotify";
import {CombinedPlayCountChartCard, PlayCountChartCard} from "./Now/PlayCountGraph";
import {ArtistBreakdownChartCard} from "./Now/ArtistBreakdownGraph";
import {LastFmLogoLink, PlayCountCard} from "./Now/LastFm";
import BaseInfoCard from "./Now/BaseInfoCard";
import {AppleLogoLink} from "./Now/Apple";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/nowhub")
    .withAutomaticReconnect()
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
    currentlyPlayingSpotify?: SpotifyCurrentlyPlayingDTO,
    currentlyPlayingApple?: AppleCurrentlyPlayingDTO,
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
            currentlyPlayingSpotify: undefined,
            currentlyPlayingApple: undefined,
            trackFeatures: undefined,
            playCount: undefined,
            cards: []
        } as NowPlaying
    },
    computed: {
        lastfmTrack() {
            return {
                name: this.currentlyPlayingSpotify.track.name,
                artist: this.currentlyPlayingSpotify.track.artists[0].name,
                album: this.currentlyPlayingSpotify.track.album.name,
                album_artist: this.currentlyPlayingSpotify.track.album.artists[0].name,
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
        trackGraphTitle() {
            return `${this.currentlyPlayingSpotify.track.name} 🎵`
        },
        albumGraphTitle() {
            return `${this.currentlyPlayingSpotify.track.album.name} 💿`
        },
        artistGraphTitle() {
            return `${this.currentlyPlayingSpotify.track.artists[0].name} 🎤`
        },
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
        connection.on("OnNewPlayingSpotify", (context: SpotifyCurrentlyPlayingDTO) =>
        {
            console.log(context);
            this.currentlyPlayingSpotify = context;
            this.trackFeatures = null;
            this.playCount = null;
            this.cards = [];

            if(context.track !== null && context.track !== undefined)
            {
                // if(context.track.id !== null)
                // {
                //     connection.invoke("SendAudioFeatures", context.track.id);
                // }
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

        connection.on("OnNewPlayingApple", (context: AppleCurrentlyPlayingDTO) => {
            console.log(context);
            this.currentlyPlayingApple = context;

            if (context.track !== null && context.track !== undefined) {
                // if(context.track.id !== null)
                // {
                //     connection.invoke("SendAudioFeatures", context.track.id);
                // }
                // connection.invoke("SendPlayCount",
                //     context.track.attributes.name,
                //     context.track.attributes.artistName,
                //     context.track.attributes.albumName,
                //     ""
                // );
                // connection.invoke("SendFacts",
                //     context.track.name,
                //     context.track.artists[0].name,
                //     context.track.album.name,
                //     context.track.album.artists[0].name
                // );
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

app.component("now-playing-card-spotify", NowPlayingCardSpotify);
app.component("now-playing-card-apple", NowPlayingCardApple);
app.component("audio-feature-card", AudioFeatureCard);
app.component("audio-feature-chart-card", AudioFeatureChartCard);
app.component("info-card", BaseInfoCard);
app.component("popularity", PopularityCard);
app.component("spotify-logo", SpotifyLogoLink);
app.component("apple-logo", AppleLogoLink);
app.component("lastfm-logo", LastFmLogoLink);
app.component("play-count-card", PlayCountCard);
app.component("play-count-chart-card", PlayCountChartCard);
app.component("play-count-chart-card-comb", CombinedPlayCountChartCard);
app.component("artist-breakdown", ArtistBreakdownChartCard);
const vm = app.mount('#app');