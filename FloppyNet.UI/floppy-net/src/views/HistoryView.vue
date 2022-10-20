<template>
  <Suspense>
    <Leaderboard />
  </Suspense>
</template>

<script>
import { defineComponent } from 'vue';

// Components
import Leaderboard from '../components/Leaderboard.vue';

export default defineComponent({
  name: 'HistoryView',

  components: {
    Leaderboard
  },
});
</script>
