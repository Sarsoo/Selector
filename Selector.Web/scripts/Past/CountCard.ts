import * as Vue from "vue";

export let CountCard: Vue.Component = {
    props: ['count'],
    computed: {
        formattedCount() {
            if(this.count != null)
            {
                return this.count.toLocaleString()
            }
            else
            {
                return '0';
            }
        }
    },
    template: 
    `
        <div class="card">
            <h2>{{ formattedCount }}</h2>
        </div>
    `
}