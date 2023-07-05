<template>
  <Suspense>
    <LineChart
      :data="{
        labels: $store.state.userStats.data.map((_, i) => `1`),
        datasets: [{
          label: 'Average',
          backgroundColor: '#6200EE',
          data: $store.state.userStats.data.map(u => u.Average)
        }, {
          label: 'Max',
          backgroundColor: '#B00020',
          data: $store.state.userStats.data.map(u => u.Max)
        },
        {
          label: 'Min',
          backgroundColor: '#03DAC6',
          data: $store.state.userStats.data.map(u => u.Min)
        }]
      }"
      :options="{
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          tooltip: {
            callbacks: {
              title: () => {
                return ``
              }
            }
          },
        },
        scales: {
          x: {
            display: false
          }
        }
      }"
      :loading="$store.state.userStats.loading" />
  </Suspense>
  <Suspense>
    <GameList :games="$store.state.userHistory.data" :loading="$store.state.userHistory.loading" :error="$store.state.userHistory.error" />
  </Suspense>
</template>

<script>
import { defineComponent } from 'vue';

// Components
import GameList from '../components/GameList.vue';
import LineChart from '../components/LineChart.vue';

export default defineComponent({
  name: 'UserView',
  components: {
    GameList,
    LineChart
  },
  created: function() {
    this.userId = this.$route.params.id;
    this.$store.dispatch('getUserHistory', { userId: this.userId });
    this.$store.dispatch('getUserStats', { userId: this.userId });
  }
});
</script>
