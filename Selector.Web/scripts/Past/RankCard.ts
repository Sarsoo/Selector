import * as Vue from "vue";

export let RankCard: Vue.Component = {
    props: ['title', 'entries'],
    computed: {
        
    },
    template: 
    `
        <div class="rank-card card">
            <h2>{{ title }}</h2>
            <ol>
                <li v-for="entry in entries">
                    {{ entry.name }} - <b>{{entry.value}}</b>
                </li>
            </ol>
        </div>
    `
}