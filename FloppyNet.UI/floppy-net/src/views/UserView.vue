<template>
  <Suspense>
    <GameList :games="$store.state.userHistory.data" :loading="$store.state.userHistory.loading" :error="$store.state.userHistory.error" />
  </Suspense>
</template>

<script>
import { defineComponent } from 'vue';

// Components
import GameList from '../components/GameList.vue';

export default defineComponent({
  name: 'UserView',
  components: {
    GameList
  },
  created: function() {
    this.userId = this.$route.params.id;
    this.$store.dispatch('getUserHistory', { userId: this.userId });
  }
});
</script>
