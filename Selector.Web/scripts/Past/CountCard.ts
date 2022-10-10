import * as Vue from "vue";

export let CountCard: Vue.Component = {
    props: ['count'],
    computed: {
        
    },
    template: 
    `
        <div class="card">
            <h2>{{ count }}</h2>
        </div>
    `
}