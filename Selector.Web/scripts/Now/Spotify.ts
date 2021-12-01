import * as Vue from "vue";
import { KeyString, ModeString } from "../Helper";
import { Chart, RadarController, RadialLinearScale, PointElement, LineElement } from "chart.js";

Chart.register(RadarController, RadialLinearScale, PointElement, LineElement);

export let PopularityCard: Vue.Component = {
    props: ['track'],
    computed: {
        progressBarWidth() {
            return `width: ${this.track.popularity}%`;
        }
    },
    template: 
    `
        <div class="card info-card">
            <h3>Popularity</h3>
            <div class="progress popularity-progress">
                <div class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" :style="progressBarWidth" :aria-valuenow="track.popularity" aria-valuemin="0" aria-valuemax="100">{{ track.popularity }}%</div>
            </div>
            <spotify-logo :link="track.externalUrls.spotify" />
        </div>
    `
}

export let SpotifyLogoLink: Vue.Component = {
    props: ['link'],
    template: 
    `
        <a :href="link" target="_blank" class="spotify-logo" v-if="link != null && link != undefined">
            <img src="/Spotify_Icon_RGB_White.png" >
        </a>

        <img src="/Spotify_Icon_RGB_White.png" class="spotify-logo" v-else>
    `
}

export let AudioFeatureCard: Vue.Component = {
    props: ['feature'],
    computed: {
        Key(): string
        {
            return KeyString(this.feature.key);
        },
        Mode(): string
        {
            return ModeString(this.feature.mode);
        }
    },
    template: 
    `
        <div class="card info-card">
            <h5>{{ Key }} {{ Mode }}</h5>
            <h5>{{ feature.tempo }} BPM</h5>
            <h5>{{ feature.timeSignature }}/4</h5>
            <h5>{{ feature.loudness }} dB</h5>
            <spotify-logo />
        </div>
    `
}

export let AudioFeatureChartCard: Vue.Component = {
    props: ['feature'],
    data() {
        return {
            chartData: {
                labels: [
                    'Energy', 
                    'Dance',
                    'Speech',
                    'Live',
                    'Instrumental',
                    'Acoustic',
                    'Valence'
                ],
                datasets: [{
                    // label: '# of Votes',
                    data: [
                        this.feature.energy, 
                        this.feature.danceability,
                        this.feature.speechiness,
                        this.feature.liveness,
                        this.feature.instrumentalness,
                        this.feature.acousticness,
                        this.feature.valence
                    ],
                }]
            }
        }
    },
    template: 
    `
        <div class="card info-card">
            <canvas id="feature-chart"></canvas>
            <spotify-logo />
        </div>
    `, 
    mounted() {
        new Chart("feature-chart", {
            type: "radar",
            data: this.chartData,
            options: {
                elements: {
                    line: {
                        borderWidth: 4,
                        borderColor: "#3a3a3a",
                        backgroundColor: "#727272",
                        borderCapStyle: "round",
                        borderJoinStyle: "round"
                    },
                    point: {
                        radius: 4,
                        pointStyle: "circle",
                        borderColor: "black",
                        backgroundColor: "white"
                    }
                },
                scales: {
                    r: {
                        angleLines: {
                            display: true
                        },
                        pointLabels: {
                            color: 'white',
                            font: {
                                size: 12
                            }
                        },
                        beginAtZero: true,
                        suggestedMin: 0,
                        suggestedMax: 1,
                        ticks: {
                            display: false
                        }
                    }
                }
            }
        })
    }
}