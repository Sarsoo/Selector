import * as Vue from "vue";
import { FullTrack } from "./HubInterfaces";

let component: Vue.Component = {
    props: ['track'],
    template: 
    `
        <div class="card now-playing-card" >
            <img :src="track.album.images[0].url">
            <h4>{{ track.name }}</h4>
            <h6>
                <template v-for="(artist, index) in track.artists">
                    <template v-if="index > 0">,</template>
                    <span>{{ artist.name }}</span>
                </template>
            </h6>
        </div>
    `
}

export default component;