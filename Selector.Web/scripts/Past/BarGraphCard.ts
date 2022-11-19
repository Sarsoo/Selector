import * as Vue from "vue";
import { Chart, BarElement, BarController } from "chart.js";
import 'chartjs-adapter-luxon';
import { CountSample, RankEntry } from "scripts/HubInterfaces";

Chart.register(BarElement, BarController);

export let BarChartCard: Vue.Component = {
    props: ['data_points', 'title', 'chart_id', 'link', 'earliest_date', 'latest_date', 'colour'],
    data() {
        return {
            chartData: {
                datasets: [{
                    data: this.data_points.map((e: RankEntry) => {
                        return {x: e.name, y: e.value};
                    }),
                }]
            }
        }
    },
    computed: {
        chartId() {
            return "bar-chart-" + this.chart_id;
        }
    },
    template: 
    `
        <div class="chart-card card" style="width: 100%">
            <h1>{{ title }}</h1>
            <canvas :id="chartId"></canvas>
            <lastfm-logo :link="link" v-if="link" />
        </div>
    `, 
    mounted() {
        new Chart(`bar-chart-${this.chart_id}`, {
            type: "bar",
            data: this.chartData,
            options: {
                backgroundColor: 'white',
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        })
    }
}