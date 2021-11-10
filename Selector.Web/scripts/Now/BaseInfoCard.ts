import * as Vue from "vue";

let BaseInfoCard: Vue.Component = {
    props: ['html'],
    template: 
    `
        <div class="card info-card" v-html="html">
        </div>
    `
}

export default BaseInfoCard;