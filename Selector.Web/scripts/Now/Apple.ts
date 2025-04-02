import * as Vue from "vue";

export let AppleLogoLink: Vue.Component = {
    props: ['link'],
    template:
        `
          <a :href="link" target="_blank" class="apple-logo" v-if="link != null && link != undefined">
            <img src="/apple.png">
          </a>

          <img src="/apple.png" class="apple-logo" v-else>
        `
}