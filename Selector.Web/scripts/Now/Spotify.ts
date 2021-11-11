import * as Vue from "vue";
import { KeyString, ModeString } from "../Helper";
import { Chart, RadarController, RadialLinearScale, PointElement, LineElement } from "chart.js";

Chart.register(RadarController, RadialLinearScale, PointElement, LineElement);

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
            <h3>Info</h3>
            <h5>Key: <b>{{ Key }} {{ Mode }}</b></h5>
            <h5>Tempo: <b>{{ feature.tempo }} BPM</b></h5>
            <h5>Time: <b>{{ feature.timeSignature }}/4</b></h5>
            <h5>Loudness: <b>{{ feature.loudness }} dB</b></h5>
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
                // plugins: {
                //     legend: {
                //         labels: {
                //             color: "white"
                //         }
                //     }
                // },
                elements: {
                    line: {
                        borderWidth: 4
                    },
                    point: {
                        radius: 4
                    }
                },
                scales: {
                    r: {
                        angleLines: {
                            display: false
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