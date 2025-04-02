import * as Vue from "vue";

let component: Vue.Component = {
    props: ['track'],
    computed: {
        IsTrackPlaying() {
            return this.track !== null && this.track !== undefined;
        },
        ImageUrl() {
            if (this.track.attributes.artwork.url) {
                return this.track.attributes.artwork.url.replace('{w}', '300').replace('{h}', '300');
            } else {
                return "";
            }
        }
    },
    template:
        `
          <div class="card now-playing-card" v-if="IsTrackPlaying">
            <img :src="ImageUrl" class="cover-art">
            <h4>{{ track.attributes.name }}</h4>
            <h6>
              {{ track.attributes.albumName }}
            </h6>
            <h6>
              {{ track.attributes.name }}
            </h6>
            <div style="width: 100%">
              <apple-logo :link="track.attributes.url" style="float: left"/>
              <img src="/live.gif" style="height: 20px; float: right">
            </div>
          </div>
        `
}

export default component;