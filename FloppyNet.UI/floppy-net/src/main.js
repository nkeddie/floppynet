import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import vuetify from './plugins/vuetify'
import store from './plugins/store'
import { loadFonts } from './plugins/webfontloader'

loadFonts()

createApp(App)
  .use(store)
  .use(router)
  .use(vuetify)
  .mount('#app')
