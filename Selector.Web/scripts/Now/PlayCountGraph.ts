import * as Vue from "vue";
import { Chart, PointElement, LineElement, LineController, CategoryScale, LinearScale, TimeSeriesScale } from "chart.js";
import { CountSample } from "scripts/HubInterfaces";

Chart.register(LineController, CategoryScale, LinearScale, TimeSeriesScale, PointElement, LineElement);

const months = ["JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"];

export let PlayCountChartCard: Vue.Component = {
    props: ['data_points', 'title', 'chart_id'],
    data() {
        return {
            chartData: {
                labels: this.data_points.map((e: CountSample) => {
                    var date = new Date(e.timeStamp);

                    return `${months[date.getMonth()]} ${date.getFullYear()}`;
                }),
                datasets: [{
                    // label: '# of Votes',
                    data: this.data_points.map((e: CountSample) => e.value),
                }]
            }
        }
    },
    computed: {
        chartId() {
            return "play-count-chart-" + this.chart_id;
        }
    },
    template: 
    `
        <div class="card info-card chart-card">
            <h1>{{ title }}</h1>
            <canvas :id="chartId"></canvas>
        </div>
    `, 
    mounted() {
        new Chart(`play-count-chart-${this.chart_id}`, {
            type: "line",
            data: this.chartData,
            options: {
                elements: {
                    line: {
                        borderWidth: 4,
                        borderColor: "#a34c77",
                        backgroundColor: "#727272",
                        borderCapStyle: "round",
                        borderJoinStyle: "round"
                    },
                    // point: {
                    //     radius: 4,
                    //     pointStyle: "circle",
                    //     borderColor: "black",
                    //     backgroundColor: "white"
                    // }
                },
                scales: {
                }
            }
        })
    }
}