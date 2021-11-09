import * as signalR from "@microsoft/signalr";
import * as Vue from "vue";
import { FullTrack, CurrentlyPlayingDTO } from "./HubInterfaces";
import NowPlayingCard from "./NowPlayingCard";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub")
    .build();

connection.start()
.then(val => {
    connection.invoke("OnConnected");
})
.catch(err => console.error(err));

interface NowPlaying {
    currentlyPlaying?: CurrentlyPlayingDTO
}

const app = Vue.createApp({
    data() {
        return {
            currentlyPlaying: {
                track: {
                    name: "No Playback",
                    album: {
                        name: "",
                        images: [
                            {
                                url: ""
                            }
                        ]
                    },
                    artists: [
                        {
                            name: ""
                        }
                    ],
                    externalUrls: {}
                }
            }
        } as NowPlaying
    },
    created() {
        connection.on("OnNewPlaying", (context: CurrentlyPlayingDTO) => 
        {
            console.log(context);
            this.currentlyPlaying = context;
        });
    }
});

app.component("now-playing-card", NowPlayingCard);
const vm = app.mount('#app');