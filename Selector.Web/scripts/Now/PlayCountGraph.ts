import * as Vue from "vue";
import { Chart, PointElement, LineElement, LineController, CategoryScale, LinearScale, TimeSeriesScale } from "chart.js";
import 'chartjs-adapter-luxon';
import { CountSample } from "scripts/HubInterfaces";
import { ScrobbleDataSeries } from "scripts/now";

Chart.register(LineController, CategoryScale, LinearScale, TimeSeriesScale, PointElement, LineElement);

const months = ["JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"];

export let PlayCountChartCard: Vue.Component = {
    props: ['data_points', 'title', 'chart_id', 'link', 'earliest_date', 'latest_date', 'colour'],
    data() {
        return {
            chartData: {
                datasets: [{
                    data: this.data_points.map((e: CountSample) => {
                        return {x: e.timeStamp, y: e.value};
                    }),
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
        <div class="chart-card card">
            <h1>{{ title }}</h1>
            <canvas :id="chartId"></canvas>
            <lastfm-logo :link="link" v-if="link" />
        </div>
    `, 
    mounted() {
        new Chart(`play-count-chart-${this.chart_id}`, {
            type: "line",
            data: this.chartData,
            options: {
                elements: {
                    line: {
                        borderWidth: 5,
                        borderColor: this.colour,
                        borderCapStyle: "round",
                        borderJoinStyle: "round"
                    },
                    point: {
                        radius: 0
                    }
                },
                scales: {
                    yAxis: {
                        suggestedMin: 0
                    },
                    xAxis: {
                        type: 'time',
                        // min: this.earliest_date,
                        // max: this.latest_date
                    }
                }
            }
        })
    }
}

export let CombinedPlayCountChartCard: Vue.Component = {
    props: ['data_points', 'title', 'chart_id', 'link', 'earliest_date'],
    data() {
        return {
            chartData: {
                datasets: this.data_points.map((series: ScrobbleDataSeries) => {
                    return {
                        label: series.label,
                        borderColor: series.colour,
                        data: series.data.map((e: CountSample) => {
                            return {x: e.timeStamp, y: e.value};
                        })
                    }
                })
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
        <div class="chart-card card">
            <canvas :id="chartId"></canvas>
            <lastfm-logo :link="link" />
        </div>
    `, 
    mounted() {
        new Chart(`play-count-chart-${this.chart_id}`, {
            type: "line",
            data: this.chartData,
            options: {
                elements: {
                    line: {
                        borderWidth: 5,
                        borderColor: "#a34c77",
                        borderCapStyle: "round",
                        borderJoinStyle: "round"
                    },
                    point: {
                        radius: 0
                    }
                },
                scales: {
                    yAxis: {
                        suggestedMin: 0
                    },
                    xAxis: {
                        type: 'time',
                        min: this.earliest_date
                    }
                }
            }
        })
    }
}
