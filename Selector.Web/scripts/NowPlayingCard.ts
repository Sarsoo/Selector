import * as Vue from "vue";

let component: Vue.Component = {
    props: ['track'],
    template: 
    `
        <div class="card now-playing-card" >
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
            <a :href="track.externalUrls.spotify" style="width: 21px">
                <img src="/Spotify_Icon_RGB_White.png" style="width: 21px">
            </a>
        </div>
    `
}

export default component;