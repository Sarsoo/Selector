import * as Vue from "vue";
import { Chart, DoughnutController, ArcElement } from "chart.js";
import { PlayCount } from "scripts/HubInterfaces";

Chart.register(DoughnutController, ArcElement);

const pieColours = ['#7a99c2',
    '#a34c77',
    '#598556',
];

export let ArtistBreakdownChartCard: Vue.Component = {
    props: ['play_count'],
    data() {
        return {

        }
    },
    computed: {
        trackPercent() {
            return this.play_count.track * 100 / this.play_count.artist
        },
        albumPercent() {
            return this.play_count.album * 100 / this.play_count.artist
        },
        albumDiff() {
            return this.albumPercent - this.trackPercent;
        },
        artistPercent() {
            return 100 - this.albumDiff + this.trackPercent;
        }
    },
    template: 
    `
        <div class="card info-card">
            <canvas id="artist-breakdown"></canvas>
            <lastfm-logo />
        </div>
    `, 
    mounted() {
        new Chart(`artist-breakdown`, {
            type: "doughnut",
            data: {
            labels: [ "track", "album", "artist" ],
            datasets: [{
                data: [ this.trackPercent, this.albumDiff, this.artistPercent ],
            }]
            },
            options: {
                plugins: {
                    legend : {
                        display : true,
                        labels: {
                            color: 'white'
                        }
                    }
                },
                layout: {
                    padding: 20
                },
                elements: {
                    arc : {
                        backgroundColor: pieColours,
                        borderWidth: 2,
                        borderColor: 'rgb(0, 0, 0)'
                    }
                }
            }
        })
    }
}