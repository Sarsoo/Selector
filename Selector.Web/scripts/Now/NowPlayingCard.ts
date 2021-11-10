import * as Vue from "vue";

let component: Vue.Component = {
    props: ['track', 'episode'],
    computed: {
        IsTrackPlaying() {
            return this.track !== null && this.track !== undefined;
        },
        IsEpisodePlaying() {
            return this.episode !== null && this.episode !== undefined;
        }
    },
    template: 
    `
        <div class="card now-playing-card" v-if="IsTrackPlaying">
            <img :src="track.album.images[0].url" class="cover-art">
            <h4>{{ track.name }}</h4>
            <h6>
                {{ track.album.name }}
            </h6>
            <h6>
                <template v-for="(artist, index) in track.artists">
                    <template v-if="index > 0">, </template>
                    <span>{{ artist.name }}</span>
                </template>
            </h6>
            <spotify-logo :link="track.externalUrls.spotify" />
        </div>

        <div class="card now-playing-card" v-else-if="IsEpisodePlaying">
            <img :src="episode.show.images[0].url" class="cover-art">
            <h4>{{ episode.name }}</h4>
            <h6>
                {{ episode.show.name }}
            </h6>
            <h6>
                {{ episode.show.publisher }}
            </h6>
            <spotify-logo :link="episode.externalUrls.spotify" />
        </div>

        <div class="card now-playing-card" v-else>
            <h4>No Playback</h4>
        </div>
    `
}

export default component;