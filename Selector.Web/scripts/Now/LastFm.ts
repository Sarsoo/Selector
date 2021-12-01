import * as Vue from "vue";

const root_url = "https://www.last.fm/user/username/library/music/artist/album";

export let PlayCountCard: Vue.Component = {
    props: ['count', 'track', 'username'],
    computed: {
        TrackLink(): string {
            return `https://www.last.fm/user/${this.username}/library/music/${this.track.artist}/_/${this.track.name}`;
        },
        AlbumLink(): string {
            return `https://www.last.fm/user/${this.username}/library/music/${this.track.album_artist}/${this.track.album}`;
        },
        ArtistLink(): string {
            return `https://www.last.fm/user/${this.username}/library/music/${this.track.artist}`;
        },
        UserLink(): string {
            return `https://www.last.fm/user/${this.username}`;
        },
        TrackPercent(): number {
            return ((this.count.track * 100) / this.count.user);
        },
        AlbumPercent(): number {
            return ((this.count.album * 100) / this.count.user);
        },
        ArtistPercent(): number {
            return ((this.count.artist * 100) / this.count.user);
        },
        TrackPercentStr(): string {
            return this.TrackPercent.toFixed(2);
        },
        AlbumPercentStr(): string {
            return this.AlbumPercent.toFixed(2);
        },
        ArtistPercentStr(): string {
            return this.ArtistPercent.toFixed(1);
        }
    },
    template: 
    `
        <div class="card info-card">
            <h5 v-if="count.track != null && count.track != undefined" >
                <a :href="TrackLink" class="subtle-link">
                    Track: {{ count.track.toLocaleString() }} <small v-if="TrackPercent >= 0.01">({{ this.TrackPercentStr }}%)</small>
                </a>
            </h5>
            <h5 v-if="count.album != null && count.album != undefined" >
                <a :href="AlbumLink" class="subtle-link">
                    Album: {{ count.album.toLocaleString() }} <small v-if="AlbumPercent >= 0.01">({{ this.AlbumPercentStr }}%)</small>
                </a>
            </h5>
            <h5 v-if="count.artist != null && count.artist != undefined" >
                <a :href="ArtistLink" class="subtle-link">
                    Artist: {{ count.artist.toLocaleString() }} <small v-if="ArtistPercent >= 0.1">({{ this.ArtistPercentStr }}%)</small>
                </a>
            </h5>
            <h5 v-if="count.user != null && count.user != undefined" >
                <a :href="UserLink" class="subtle-link">
                    User: {{ count.user.toLocaleString() }}
                </a>
            </h5>
            <lastfm-logo :link="UserLink" />
        </div>
    `
}

export let LastFmLogoLink: Vue.Component = {
    props: ['link'],
    template:
        `
        <a :href="link" target="_blank" class="lastfm-logo" v-if="link != null && link != undefined">
            <img src="/last-fm.png" >
        </a>

        <img src="/last-fm.png" class="lastfm-logo" v-else>
    `
}