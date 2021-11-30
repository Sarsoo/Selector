import * as Vue from "vue";

export let PlayCountCard: Vue.Component = {
    props: ['count'],
    template: 
    `
        <div class="card info-card">
            <h5 v-if="count.track != null && count.track != undefined" >Track: {{ count.track.toLocaleString() }}</h5>
            <h5 v-if="count.album != null && count.album != undefined" >Album: {{ count.album.toLocaleString() }}</h5>
            <h5 v-if="count.artist != null && count.artist != undefined" >Artist: {{ count.artist.toLocaleString() }}</h5>
            <h5 v-if="count.user != null && count.user != undefined" >User: {{ count.user.toLocaleString() }}</h5>
        </div>
    `
}