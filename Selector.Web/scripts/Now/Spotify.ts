import * as Vue from "vue";

export let PopularityCard: Vue.Component = {
    props: ['track'],
    template: 
    `
        <div class="card info-card">
            <h3>Popularity</h3>
            <h1>{{ track.popularity }}%</h1>
            <spotify-logo :link="track.externalUrls.spotify" />
        </div>
    `
}

export let SpotifyLogoLink: Vue.Component = {
    props: ['link'],
    template: 
    `
        <a :href="link" class="spotify-logo" v-if="link != null && link != undefined">
            <img src="/Spotify_Icon_RGB_White.png">
        </a>

        <img src="/Spotify_Icon_RGB_White.png" v-else>
    `
}