<template>
  <Suspense>
    <Leaderboard />
  </Suspense>
  
  <Suspense>
    <GameList :games="$store.state.groupHistory.data" :loading="$store.state.groupHistory.loading" :error="$store.state.groupHistory.error" />
  </Suspense>
</template>

<script>
import { defineComponent } from 'vue';

// Components
import Leaderboard from '../components/Leaderboard.vue';
import GameList from '../components/GameList.vue';

export default defineComponent({
  name: 'HomeView',

  components: {
    Leaderboard,
    GameList
  },

  created: function() {
    this.$store.dispatch('getGroupHistory');
  }
});
</script>
