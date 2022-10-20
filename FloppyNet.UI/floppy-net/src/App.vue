<template>
  <v-app>
    <v-main>
      <v-app-bar color="primary" density="compact">
        <template v-slot:prepend>
          <v-app-bar-nav-icon @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
        </template>

        <v-app-bar-title>Wordle</v-app-bar-title>
      </v-app-bar>
      <v-navigation-drawer
        v-model="drawer"
        bottom
        temporary
      >
        <v-list>
          <v-list-item
              v-for="item in items"
              :key="item.title"
              dense
              router :to="item.route"
          >
              <v-list-item-icon>
                  <v-icon>{{ item.icon }}</v-icon>
              </v-list-item-icon>
              <v-list-item-content>
                  <v-list-title>{{ item.title }}</v-list-title>
              </v-list-item-content>

          </v-list-item>
        </v-list>
      </v-navigation-drawer>
      <router-view/>
    </v-main>
  </v-app>
</template>

<script>

export default {
  name: 'App',

  data: () => ({
    drawer: false,
    items: [
      { title: 'Wordle', value: 'Wordle', icon: 'mdi-file-word-box', route: '/' }
    ]
  }),

  created: function() {
    this.$store.dispatch('getUsers');
  }
}
</script>
