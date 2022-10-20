<template>
  <v-container>
    <v-card>
      <v-progress-linear v-if="$props.loading" class="position-absolute" indeterminate />
      <v-card-title>
        <v-icon>mdi-gamepad-variant-outline</v-icon> Recent Games
      </v-card-title>
      <v-card-text>
        <v-container>
          <v-row>
            <v-col cols="6" v-for="game in $props.games">
              <b>{{$store.getters.getUsername(game.UserId)}}</b>
              <br />
              Wordle {{game.WordleId}}
              <pre>{{game.GuessMatrix}}</pre>
            </v-col>
          </v-row>
        </v-container>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script>
export default {
  name: 'GameList',
  props: {
    games: Array,
    loading: Boolean,
    error: String,
  }
}
</script>
