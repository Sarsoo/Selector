import * as Vue from "vue";

let component: Vue.Component = {
    props: ['track', 'episode'],
    computed: {
        IsTrackPlaying() {
            return this.track !== null && this.track !== undefined;
        },
        IsEpisodePlaying() {
            return this.episode !== null && this.episode !== undefined;
        },
        ImageUrl() {
            if(this.track.album.images.length > 0)
            {
                return this.track.album.images[0].url;
            }
            else{
                return "";
            }
        }
    },
    template: 
    `
        <div class="card now-playing-card" v-if="IsTrackPlaying">
            <img :src="ImageUrl" class="cover-art">
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
            <div style="width: 100%">
                <spotify-logo :link="track.externalUrls.spotify" style="float: left" />
                <img src="/live.gif" style="height: 20px; float: right">
            </div>
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
            <div style="width: 100%">
                <spotify-logo :link="episode.externalUrls.spotify" style="float: left" />
                <img src="/live.gif" style="height: 20px; float: right">
            </div>
        </div>
    `
}

export default component;