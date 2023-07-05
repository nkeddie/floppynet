<template>
  <Suspense>
    <LineChart
      :data="{
        labels: bucketize($store.state.userStats.data, d => d.Average, average),
        datasets: [{
          label: 'Average',
          backgroundColor: '#6200EE',
          data: bucketize($store.state.userStats.data, d => d.Average, average)
        }, {
          label: 'Max',
          backgroundColor: '#B00020',
          data: bucketize($store.state.userStats.data, d => d.Max, max)
        },
        {
          label: 'Min',
          backgroundColor: '#03DAC6',
          data: bucketize($store.state.userStats.data, d => d.Min, min)
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
  methods: {
    average: (data) => data.reduce((a, b) => a + b) / data.length,
    max: (data) => data.reduce((a, b) => a > b ? a : b),
    min: (data) => data.reduce((a, b) => a < b ? a : b),
    bucketize(data, selector, aggregationFunction) {
      const bucketSize = 60

      const itemsPerBucket = Math.round(data.length / bucketSize)

      if (itemsPerBucket <= 1)
        return data.map(selector);

      var result = [];
      var index = 0;

      for (var i = 0; i < bucketSize; i++) {
        var temp = []
        for (var j = 0; j < itemsPerBucket; j++) {
          if (index < data.length) {
            temp.push(data[index])
            index++;
          }
        }
        if (temp.length > 0) {
          result.push(aggregationFunction(temp.map(selector)))
        }
      }

      return result
    }
  },
  created: function() {
    this.userId = this.$route.params.id;
    this.$store.dispatch('getUserHistory', { userId: this.userId });
    this.$store.dispatch('getUserStats', { userId: this.userId });
  }
});
</script>
