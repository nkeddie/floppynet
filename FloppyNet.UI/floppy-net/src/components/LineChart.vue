<template>
  <v-container>
    <v-card>
      <v-progress-linear v-if="$props.loading" class="position-absolute" indeterminate />
      <v-card-title>
        <v-icon>mdi-chart-line</v-icon> Stats
      </v-card-title>
      <v-card-text>
        <v-container>
          <v-row>
              <Line :data="data" :options="options" />
          </v-row>
        </v-container>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script>
import { Line } from 'vue-chartjs'
import { Chart as ChartJS, Title, Tooltip, Legend, LineElement, CategoryScale, LinearScale, PointElement } from 'chart.js'

ChartJS.register(Title, Tooltip, Legend, LineElement, CategoryScale, LinearScale, PointElement)

export default {
  name: 'LineChart',
  components: { Line },
  props: {
    data: {
      type: Object,
      required: true,
    },
    options: {
      type: Object,
      default: () => {}
    },
    loading: Boolean,
    error: String,
  }
}
</script>